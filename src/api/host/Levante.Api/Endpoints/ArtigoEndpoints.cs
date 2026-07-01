using Levante.Conteudo.Application.Artigos;
using Levante.Conteudo.Application.Artigos.ListarArtigosPublicados;
using Levante.Conteudo.Application.Artigos.ObterArtigoPorSlug;

namespace Levante.Api.Endpoints;

/// <summary>Endpoints publicos de artigos (contexto Conteudo).</summary>
public static class ArtigoEndpoints
{
    public static IEndpointRouteBuilder MapArtigoEndpoints(this IEndpointRouteBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        var grupo = app.MapGroup("/artigos").WithTags("Artigos");

        grupo.MapGet("/", ListarPublicados)
            .AllowAnonymous() // endpoint publico: decisao de autorizacao explicita
            .WithName("ListarArtigosPublicados")
            .Produces<IReadOnlyList<ArtigoResponse>>();

        grupo.MapGet("/{slug}", ObterPorSlug)
            .AllowAnonymous() // endpoint publico: decisao de autorizacao explicita
            .WithName("ObterArtigoPorSlug")
            .Produces<ArtigoResponse>()
            .Produces(StatusCodes.Status404NotFound);

        return app;
    }

    private static async Task<IResult> ListarPublicados(
        ListarArtigosPublicadosQueryHandler handler,
        CancellationToken ct)
    {
        var resultado = await handler.Handle(new ListarArtigosPublicadosQuery(), ct);

        return resultado.Sucesso
            ? Results.Ok(resultado.Valor)
            : ResultadoHttp.Falha(resultado.Erro);
    }

    private static async Task<IResult> ObterPorSlug(
        string slug,
        ObterArtigoPorSlugQueryHandler handler,
        CancellationToken ct)
    {
        var resultado = await handler.Handle(new ObterArtigoPorSlugQuery(slug), ct);

        return resultado.Sucesso
            ? Results.Ok(resultado.Valor)
            : Results.NotFound();
    }
}
