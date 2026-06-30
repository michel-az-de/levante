using Levante.Conteudo.Application.Artigos;
using Levante.Conteudo.Application.Artigos.ListarArtigosPublicadosPorCategoria;
using Levante.Conteudo.Application.Categorias;
using Levante.Conteudo.Application.Categorias.CriarCategoria;
using Levante.Conteudo.Application.Categorias.EditarCategoria;
using Levante.Conteudo.Application.Categorias.ListarCategorias;

namespace Levante.Api.Endpoints;

/// <summary>
/// Endpoints de categorias (taxonomia). Leitura publica (editor, browse, render);
/// escrita protegida por JWT.
/// </summary>
public static class CategoriaEndpoints
{
    public static IEndpointRouteBuilder MapCategoriaEndpoints(this IEndpointRouteBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        var grupo = app.MapGroup("/categorias").WithTags("Categorias");

        grupo.MapGet("/", Listar)
            .AllowAnonymous() // publico: usado pelo editor, browse e render
            .WithName("ListarCategorias")
            .Produces<IReadOnlyList<CategoriaResponse>>();

        grupo.MapGet("/{slug}/artigos", ListarArtigosPorCategoria)
            .AllowAnonymous() // publico: browse por categoria (so publicados)
            .WithName("ListarArtigosPorCategoria")
            .Produces<IReadOnlyList<ArtigoResponse>>()
            .Produces(StatusCodes.Status404NotFound);

        grupo.MapPost("/", Criar)
            .RequireAuthorization()
            .WithName("CriarCategoria")
            .Produces<CategoriaResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status409Conflict);

        grupo.MapPut("/{id:guid}", Editar)
            .RequireAuthorization()
            .WithName("EditarCategoria")
            .Produces<CategoriaResponse>()
            .ProducesValidationProblem()
            .Produces(StatusCodes.Status404NotFound);

        return app;
    }

    private static async Task<IResult> Listar(ListarCategoriasQueryHandler handler, CancellationToken ct)
    {
        var resultado = await handler.Handle(new ListarCategoriasQuery(), ct);
        return resultado.Sucesso ? Results.Ok(resultado.Valor) : ResultadoHttp.Falha(resultado.Erro);
    }

    private static async Task<IResult> ListarArtigosPorCategoria(
        string slug,
        ListarArtigosPublicadosPorCategoriaQueryHandler handler,
        CancellationToken ct)
    {
        var resultado = await handler.Handle(new ListarArtigosPublicadosPorCategoriaQuery(slug), ct);
        return resultado.Sucesso ? Results.Ok(resultado.Valor) : ResultadoHttp.Falha(resultado.Erro);
    }

    private static async Task<IResult> Criar(
        CriarCategoriaRequest requisicao,
        CriarCategoriaCommandHandler handler,
        CancellationToken ct)
    {
        var resultado = await handler.Handle(
            new CriarCategoriaCommand(requisicao.Nome, requisicao.Slug, requisicao.Descricao), ct);

        return resultado.Sucesso
            ? Results.Created($"/categorias/{resultado.Valor!.Slug}", resultado.Valor)
            : ResultadoHttp.Falha(resultado.Erro);
    }

    private static async Task<IResult> Editar(
        Guid id,
        EditarCategoriaRequest requisicao,
        EditarCategoriaCommandHandler handler,
        CancellationToken ct)
    {
        var resultado = await handler.Handle(
            new EditarCategoriaCommand(id, requisicao.Nome, requisicao.Descricao), ct);

        return resultado.Sucesso ? Results.Ok(resultado.Valor) : ResultadoHttp.Falha(resultado.Erro);
    }
}

/// <summary>Corpo de criacao de categoria (slug informado pelo admin).</summary>
public sealed record CriarCategoriaRequest(string Nome, string Slug, string? Descricao = null);

/// <summary>Corpo de edicao de categoria (Id vem da rota; slug imutavel).</summary>
public sealed record EditarCategoriaRequest(string Nome, string? Descricao = null);
