namespace Levante.SharedKernel.Infrastructure.Hiram;

/// <summary>Classe do resultado de um POST no Hiram, do ponto de vista do relay.</summary>
internal enum ClasseResultado
{
    /// <summary>202 (com/sem replay) ou 409: aceito pelo Hiram → marca Enviada.</summary>
    Aceito,

    /// <summary>5xx / 408 / 429 / timeout / rede: reprocessar por backoff (segue Pendente).</summary>
    Transitoria,

    /// <summary>4xx nao-recuperavel (400/401/403/404/422): marca Falhada, nao re-tenta.</summary>
    Permanente,
}

/// <summary>
/// Resultado tipado de uma emissao. <see cref="RetryAfter"/> carrega o header
/// <c>Retry-After</c> (429) ate o relay, que decide o <c>proximaTentativaEm</c>.
/// </summary>
internal sealed record ResultadoEmissao(ClasseResultado Classe, TimeSpan? RetryAfter = null);

/// <summary>
/// Cliente do Hiram (<c>POST /v1/events</c>). NAO faz retry — so classifica a resposta;
/// todo o retry mora no relay (uma unica fonte de verdade, auditoria fiel de tentativas).
/// </summary>
internal interface IHiramClient
{
    Task<ResultadoEmissao> EnviarAsync(HiramEventRequest requisicao, CancellationToken ct);
}
