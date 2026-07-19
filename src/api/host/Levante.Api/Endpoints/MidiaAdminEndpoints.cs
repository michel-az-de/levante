using Levante.Conteudo.Application.Midias;
using Levante.Conteudo.Application.Midias.EnviarMidia;
using Levante.Conteudo.Application.Midias.RemoverMidia;
using Microsoft.AspNetCore.Mvc;

namespace Levante.Api.Endpoints;

/// <summary>Endpoints administrativos de midia (contexto Conteudo). Todos exigem autorizacao.</summary>
public static class MidiaAdminEndpoints
{
    /// <summary>
    /// Nome do campo multipart esperado no upload. Precisa ser identico ao nome do
    /// parametro <c>arquivo</c> de <see cref="Enviar"/>: e por ele que o minimal API
    /// liga o IFormFile.
    /// </summary>
    public const string CampoArquivo = "arquivo";

    // Margem sobre o limite de negocio (EnviarMidiaCommandValidator) para cobrir o
    // overhead do envelope multipart (boundary, headers de parte, etc.).
    private const long LimiteRequisicaoBytes = EnviarMidiaCommandValidator.TamanhoMaximoBytes + 512 * 1024;

    public static IEndpointRouteBuilder MapMidiaAdminEndpoints(this IEndpointRouteBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        var midias = app.MapGroup("/admin/midias").WithTags("Midias (admin)").RequireAuthorization();

        midias.MapPost("/", Enviar)
            .WithName("EnviarMidia")
            // Metadata declarativa: o Kestrel corta o corpo antes de o handler rodar,
            // entao o 413 vem do transporte e nao de validacao apos bufferizar tudo.
            .WithMetadata(new RequestSizeLimitAttribute(LimiteRequisicaoBytes))
            .DisableAntiforgery()
            .Produces<MidiaResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status413PayloadTooLarge);

        midias.MapDelete("/{id:guid}", Remover)
            .WithName("RemoverMidia")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

        return app;
    }

    private static async Task<IResult> Enviar(
        IFormFile? arquivo, EnviarMidiaCommandHandler handler, CancellationToken ct)
    {
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

    private static async Task<IResult> Remover(
        Guid id, RemoverMidiaCommandHandler handler, CancellationToken ct)
    {
        var resultado = await handler.Handle(new RemoverMidiaCommand(id), ct);

        return resultado.Sucesso ? Results.NoContent() : ResultadoHttp.Falha(resultado.Erro);
    }
}
