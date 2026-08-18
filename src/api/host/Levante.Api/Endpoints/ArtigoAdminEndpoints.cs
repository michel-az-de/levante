using Levante.Conteudo.Application.Artigos;
using Levante.Conteudo.Application.Artigos.ArquivarArtigo;
using Levante.Conteudo.Application.Artigos.CriarArtigo;
using Levante.Conteudo.Application.Artigos.EditarArtigo;
using Levante.Conteudo.Application.Artigos.ListarTodosArtigos;
using Levante.Conteudo.Application.Artigos.PublicarArtigo;

namespace Levante.Api.Endpoints;

/// <summary>
/// Endpoints administrativos de artigos (contexto Conteudo). Todos exigem
/// autorizacao (JWT da Fatia 2a). Escrita e gestao de status do conteudo.
/// </summary>
public static class ArtigoAdminEndpoints
{
    public static IEndpointRouteBuilder MapArtigoAdminEndpoints(this IEndpointRouteBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        var artigos = app.MapGroup("/artigos").WithTags("Artigos (admin)").RequireAuthorization();

        artigos.MapPost("/", Criar)
            .WithName("CriarArtigo")
            .Produces<ArtigoResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status409Conflict);

        artigos.MapPut("/{id:guid}", Editar)
            .WithName("EditarArtigo")
            .Produces<ArtigoResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        artigos.MapPost("/{id:guid}/publicar", Publicar)
            .WithName("PublicarArtigo")
            .Produces<ArtigoResponse>()
            .Produces(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        artigos.MapPost("/{id:guid}/arquivar", Arquivar)
            .WithName("ArquivarArtigo")
            .Produces<ArtigoResponse>()
            .Produces(StatusCodes.Status404NotFound);

        app.MapGet("/admin/artigos", ListarTodos)
            .RequireAuthorization()
            .WithTags("Artigos (admin)")
            .WithName("ListarTodosArtigos")
            .Produces<IReadOnlyList<ArtigoResponse>>();

        return app;
    }

    private static async Task<IResult> Criar(
        CriarArtigoRequest requisicao,
        CriarArtigoCommandHandler handler,
        CancellationToken ct)
    {
        var resultado = await handler.Handle(requisicao.ParaCriarCommand(), ct);

        return resultado.Sucesso
            ? Results.Created($"/artigos/{resultado.Valor!.Slug}", resultado.Valor)
            : ResultadoHttp.Falha(resultado.Erro);
    }

    private static async Task<IResult> Editar(
        Guid id,
        EditarArtigoRequest requisicao,
        EditarArtigoCommandHandler handler,
        CancellationToken ct)
    {
        var resultado = await handler.Handle(requisicao.ParaEditarCommand(id), ct);

        return resultado.Sucesso ? Results.Ok(resultado.Valor) : ResultadoHttp.Falha(resultado.Erro);
    }

    private static async Task<IResult> Publicar(
        Guid id,
        PublicarArtigoCommandHandler handler,
        CancellationToken ct)
    {
        var resultado = await handler.Handle(new PublicarArtigoCommand(id), ct);

        return resultado.Sucesso ? Results.Ok(resultado.Valor) : ResultadoHttp.Falha(resultado.Erro);
    }

    private static async Task<IResult> Arquivar(
        Guid id,
        ArquivarArtigoCommandHandler handler,
        CancellationToken ct)
    {
        var resultado = await handler.Handle(new ArquivarArtigoCommand(id), ct);

        return resultado.Sucesso ? Results.Ok(resultado.Valor) : ResultadoHttp.Falha(resultado.Erro);
    }

    private static async Task<IResult> ListarTodos(
        ListarTodosArtigosQueryHandler handler,
        CancellationToken ct)
    {
        var resultado = await handler.Handle(new ListarTodosArtigosQuery(), ct);

        return resultado.Sucesso ? Results.Ok(resultado.Valor) : ResultadoHttp.Falha(resultado.Erro);
    }

}

/// <summary>Corpo de criacao de artigo (slug informado pelo admin). Meta SEO, categoria e tags opcionais.</summary>
public sealed record CriarArtigoRequest(
    string Titulo,
    string Slug,
    string Resumo,
    string Conteudo,
    string? MetaTitulo = null,
    string? MetaDescricao = null,
    string? ImagemOgUrl = null,
    Guid? CategoriaId = null,
    IReadOnlyList<string>? Tags = null) : IDadosDeArtigo;

/// <summary>Corpo de edicao de artigo (Id vem da rota). Meta SEO, categoria e tags opcionais.</summary>
public sealed record EditarArtigoRequest(
    string Titulo,
    string Slug,
    string Resumo,
    string Conteudo,
    string? MetaTitulo = null,
    string? MetaDescricao = null,
    string? ImagemOgUrl = null,
    Guid? CategoriaId = null,
    IReadOnlyList<string>? Tags = null) : IDadosDeArtigo;

/// <summary>
/// Campos comuns de criacao e edicao de artigo. Existe para deduplicar o mapeamento
/// request->command num lugar so, SEM unificar os dois records: a convencao de nomes
/// pede DTO com verbo (Criar/Editar...Request), entao eles seguem separados no contrato.
/// </summary>
internal interface IDadosDeArtigo
{
    string Titulo { get; }
    string Slug { get; }
    string Resumo { get; }
    string Conteudo { get; }
    string? MetaTitulo { get; }
    string? MetaDescricao { get; }
    string? ImagemOgUrl { get; }
    Guid? CategoriaId { get; }
    IReadOnlyList<string>? Tags { get; }
}

/// <summary>Mapeia os dados comuns do request para o command correspondente (elimina a copia campo a campo).</summary>
internal static class MapeamentoDeArtigo
{
    public static CriarArtigoCommand ParaCriarCommand(this IDadosDeArtigo dados) =>
        new(dados.Titulo, dados.Slug, dados.Resumo, dados.Conteudo,
            dados.MetaTitulo, dados.MetaDescricao, dados.ImagemOgUrl, dados.CategoriaId, dados.Tags);

    public static EditarArtigoCommand ParaEditarCommand(this IDadosDeArtigo dados, Guid id) =>
        new(id, dados.Titulo, dados.Slug, dados.Resumo, dados.Conteudo,
            dados.MetaTitulo, dados.MetaDescricao, dados.ImagemOgUrl, dados.CategoriaId, dados.Tags);
}
