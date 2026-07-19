using Levante.Conteudo.Application.Midias.ObterMidia;
using Microsoft.Net.Http.Headers;

namespace Levante.Api.Endpoints;

/// <summary>Endpoint publico de leitura de midia (contexto Conteudo).</summary>
public static class MidiaEndpoints
{
    public static IEndpointRouteBuilder MapMidiaEndpoints(this IEndpointRouteBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        app.MapGet("/midias/{id:guid}", Obter)
            .AllowAnonymous() // endpoint publico: decisao de autorizacao explicita
            .WithTags("Midias")
            .WithName("ObterMidia")
            .Produces(StatusCodes.Status200OK, contentType: "application/octet-stream")
            .Produces(StatusCodes.Status304NotModified)
            .Produces(StatusCodes.Status404NotFound);

        return app;
    }

    private static async Task<IResult> Obter(
        Guid id, HttpResponse resposta, ObterMidiaQueryHandler handler, CancellationToken ct)
    {
        var resultado = await handler.Handle(new ObterMidiaQuery(id), ct);
        if (resultado.Falhou)
        {
            return Results.NotFound();
        }

        var midia = resultado.Valor!;

        // Id de midia e imutavel (novo upload = novo id, nunca reusado):
        // cache agressivo no browser/CDN e seguro.
        resposta.Headers.CacheControl = "public, max-age=31536000, immutable";

        // entityTag habilita o 304 automatico (If-None-Match) via o mesmo
        // executor de FileResult usado por Results.File/Results.Stream.
        return Results.Stream(
            midia.Conteudo,
            midia.ContentType,
            lastModified: null,
            entityTag: new EntityTagHeaderValue($"\"{id}\""),
            enableRangeProcessing: false);
    }
}
