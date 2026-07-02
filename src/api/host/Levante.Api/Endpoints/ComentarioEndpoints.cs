using Levante.Api.Seguranca;
using Levante.Engajamento.Application.Comentarios;
using Levante.Engajamento.Application.Comentarios.AprovarComentario;
using Levante.Engajamento.Application.Comentarios.CriarComentario;
using Levante.Engajamento.Application.Comentarios.ListarComentariosAprovados;
using Levante.Engajamento.Application.Comentarios.ListarComentariosPendentes;
using Levante.Engajamento.Application.Comentarios.RejeitarComentario;

namespace Levante.Api.Endpoints;

/// <summary>
/// Endpoints de comentario (contexto Engajamento). Leitura publica so de aprovados;
/// criacao publica (anonima, honeypot, rate limit) nasce Pendente; moderacao exige JWT.
/// </summary>
public static class ComentarioEndpoints
{
    public static IEndpointRouteBuilder MapComentarioEndpoints(this IEndpointRouteBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        var publico = app.MapGroup("/artigos/{id:guid}/comentarios").WithTags("Comentarios");

        publico.MapGet("/", ListarAprovados)
            .AllowAnonymous() // publico: so comentarios aprovados
            .WithName("ListarComentariosAprovados")
            .Produces<IReadOnlyList<ComentarioResponse>>();

        publico.MapPost("/", Criar)
            .AllowAnonymous()
            .RequireRateLimiting(RateLimiting.PolicyPublico)
            .WithName("CriarComentario")
            .Produces(StatusCodes.Status202Accepted)
            .ProducesProblem(StatusCodes.Status400BadRequest);

        var admin = app.MapGroup("/admin/comentarios").WithTags("Comentarios (admin)").RequireAuthorization();

        admin.MapGet("/", ListarPendentes)
            .WithName("ListarComentariosPendentes")
            .Produces<IReadOnlyList<ComentarioResponse>>();

        admin.MapPost("/{id:guid}/aprovar", Aprovar)
            .WithName("AprovarComentario")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        admin.MapPost("/{id:guid}/rejeitar", Rejeitar)
            .WithName("RejeitarComentario")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        return app;
    }

    private static async Task<IResult> ListarAprovados(
        Guid id, ListarComentariosAprovadosQueryHandler handler, CancellationToken ct)
    {
        var resultado = await handler.Handle(new ListarComentariosAprovadosQuery(id), ct);
        return resultado.Sucesso ? Results.Ok(resultado.Valor) : ResultadoHttp.Falha(resultado.Erro);
    }

    private static async Task<IResult> Criar(
        Guid id,
        CriarComentarioRequest requisicao,
        HttpContext contexto,
        CriarComentarioCommandHandler handler,
        CancellationToken ct)
    {
        var resultado = await handler.Handle(
            new CriarComentarioCommand(
                id,
                requisicao.ArtigoSlug,
                requisicao.Autor,
                requisicao.Texto,
                OrigemDoCliente.Visitante(contexto),
                OrigemDoCliente.Ip(contexto),
                OrigemDoCliente.UserAgent(contexto),
                requisicao.Armadilha),
            ct);

        // 202: aceito para moderacao (nao aparece ate ser aprovado).
        return resultado.Sucesso ? Results.Accepted() : ResultadoHttp.Falha(resultado.Erro);
    }

    private static async Task<IResult> ListarPendentes(
        ListarComentariosPendentesQueryHandler handler, CancellationToken ct)
    {
        var resultado = await handler.Handle(new ListarComentariosPendentesQuery(), ct);
        return resultado.Sucesso ? Results.Ok(resultado.Valor) : ResultadoHttp.Falha(resultado.Erro);
    }

    private static async Task<IResult> Aprovar(Guid id, AprovarComentarioCommandHandler handler, CancellationToken ct)
    {
        var resultado = await handler.Handle(new AprovarComentarioCommand(id), ct);
        return resultado.Sucesso ? Results.Ok() : ResultadoHttp.Falha(resultado.Erro);
    }

    private static async Task<IResult> Rejeitar(Guid id, RejeitarComentarioCommandHandler handler, CancellationToken ct)
    {
        var resultado = await handler.Handle(new RejeitarComentarioCommand(id), ct);
        return resultado.Sucesso ? Results.Ok() : ResultadoHttp.Falha(resultado.Erro);
    }
}

/// <summary>Corpo do POST de comentario. <c>Armadilha</c> e o honeypot (deve vir vazio).</summary>
public sealed record CriarComentarioRequest(
    string ArtigoSlug,
    string Autor,
    string Texto,
    string? Armadilha = null);
