using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Levante.SharedKernel.Infrastructure.Hiram;

/// <summary>
/// Cliente tipado fino do Hiram. <c>X-Api-Key</c> vem do <see cref="HttpClient"/> (DI);
/// <c>Idempotency-Key</c> = eventId por requisicao (o dedupe real do Hiram e o eventId
/// do corpo, estavel entre retentativas). Classifica a resposta em
/// <see cref="ClasseResultado"/>; timeout/rede/broken-circuit viram <c>Transitoria</c>.
/// </summary>
internal sealed class HiramClient(HttpClient http) : IHiramClient
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    public async Task<ResultadoEmissao> EnviarAsync(HiramEventRequest requisicao, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(requisicao);

        using var mensagem = new HttpRequestMessage(HttpMethod.Post, "/v1/events")
        {
            Content = JsonContent.Create(requisicao, options: Json),
        };
        mensagem.Headers.TryAddWithoutValidation("Idempotency-Key", requisicao.EventId);

        HttpResponseMessage resposta;
        try
        {
            resposta = await http.SendAsync(mensagem, ct);
        }
        catch (HttpRequestException)
        {
            // Rede indisponivel / DNS / conexao recusada: reprocessavel.
            return new ResultadoEmissao(ClasseResultado.Transitoria);
        }
        catch (TaskCanceledException) when (!ct.IsCancellationRequested)
        {
            // Timeout do HttpClient (nao cancelamento do relay): reprocessavel.
            return new ResultadoEmissao(ClasseResultado.Transitoria);
        }

        using (resposta)
        {
            var codigo = (int)resposta.StatusCode;

            // 202 (com/sem Idempotency-Replayed) e 409 = aceito. O replay real do Hiram
            // e 202+header; 409 nao ocorre nesta rota, mas e seguro tratar como sucesso.
            if (resposta.StatusCode is HttpStatusCode.Accepted or HttpStatusCode.Conflict)
            {
                return new ResultadoEmissao(ClasseResultado.Aceito);
            }

            if (codigo is 408 or 429 || codigo >= 500)
            {
                return new ResultadoEmissao(ClasseResultado.Transitoria, resposta.Headers.RetryAfter?.Delta);
            }

            // Demais 4xx (400/401/403/404/422): malformado/negado, nao adianta re-tentar.
            return new ResultadoEmissao(ClasseResultado.Permanente);
        }
    }
}
