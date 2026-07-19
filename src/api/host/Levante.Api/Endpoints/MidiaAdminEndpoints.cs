using Levante.Conteudo.Application.Midias;
using Levante.Conteudo.Application.Midias.EnviarMidia;
using Microsoft.AspNetCore.Http.Features;

namespace Levante.Api.Endpoints;

/// <summary>Endpoint administrativo de upload de midia (contexto Conteudo). Exige autorizacao.</summary>
public static class MidiaAdminEndpoints
{
    /// <summary>Nome do campo multipart esperado no upload.</summary>
    public const string CampoArquivo = "arquivo";

    // Margem sobre o limite de negocio (EnviarMidiaCommandValidator) para cobrir o
    // overhead do envelope multipart (boundary, headers de parte, etc.).
    private const long LimiteRequisicaoBytes = EnviarMidiaCommandValidator.TamanhoMaximoBytes + 512 * 1024;

    public static IEndpointRouteBuilder MapMidiaAdminEndpoints(this IEndpointRouteBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        app.MapPost("/admin/midias", Enviar)
            .RequireAuthorization()
            .WithTags("Midias (admin)")
            .WithName("EnviarMidia")
            .Accepts<IFormFile>("multipart/form-data")
            .Produces<MidiaResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status413PayloadTooLarge);

        return app;
    }

    private static async Task<IResult> Enviar(
        HttpRequest requisicao, EnviarMidiaCommandHandler handler, CancellationToken ct)
    {
        // O limite precisa ser setado ANTES de ler o corpo (ReadFormAsync): so
        // por isso o parametro e HttpRequest cru, e nao um IFormFile ligado
        // automaticamente (o binding automatico leria o form antes do handler
        // rodar, tarde demais para este limite valer).
        var limite = requisicao.HttpContext.Features.Get<IHttpMaxRequestBodySizeFeature>();
        if (limite is not null && !limite.IsReadOnly)
        {
            limite.MaxRequestBodySize = LimiteRequisicaoBytes;
        }

        if (!requisicao.HasFormContentType)
        {
            return Results.BadRequest();
        }

        IFormCollection formulario;
        try
        {
            formulario = await requisicao.ReadFormAsync(ct);
        }
        catch (BadHttpRequestException ex)
        {
            return Results.StatusCode(ex.StatusCode);
        }
        catch (InvalidDataException)
        {
            // Multipart malformado (ex.: corpo sem nenhuma parte) - o
            // MultipartReader lanca InvalidDataException, nao BadHttpRequestException.
            // Entrada de cliente invalida e sempre 400, nunca 500.
            return Results.BadRequest();
        }

        var arquivo = formulario.Files.GetFile(CampoArquivo);
        if (arquivo is null || arquivo.Length == 0)
        {
            return Results.BadRequest();
        }

        await using var conteudo = arquivo.OpenReadStream();
        var resultado = await handler.Handle(
            new EnviarMidiaCommand(conteudo, arquivo.ContentType, arquivo.FileName, arquivo.Length), ct);

        return resultado.Sucesso
            ? Results.Created(resultado.Valor!.Url, resultado.Valor)
            : ResultadoHttp.Falha(resultado.Erro);
    }
}
