using Levante.Api.Seguranca;
using Levante.Engajamento.Application.Reacoes;
using Levante.Engajamento.Application.Reacoes.ObterReacoesDoArtigo;
using Levante.Engajamento.Application.Reacoes.RegistrarReacao;
using Levante.Engajamento.Application.Reacoes.RemoverReacao;
using Levante.Engajamento.Domain.Reacoes;

namespace Levante.Api.Endpoints;

/// <summary>
/// Endpoints publicos de reacao a artigos (contexto Engajamento). Anonimo: a
/// identidade vem do BFF do Next (id de visitante em X-Visitante, IP em
/// X-Forwarded-For). Escrita com rate limit por IP.
/// </summary>
public static class ReacaoEndpoints
{
    public static IEndpointRouteBuilder MapReacaoEndpoints(this IEndpointRouteBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        var grupo = app.MapGroup("/artigos/{id:guid}/reacoes").WithTags("Reacoes");

        grupo.MapGet("/", Obter)
            .AllowAnonymous() // publico: contagens + reacoes do visitante atual
            .WithName("ObterReacoes")
            .Produces<ReacoesResponse>();

        grupo.MapPost("/", Registrar)
            .AllowAnonymous()
            .RequireRateLimiting(RateLimiting.PolicyPublico)
            .WithName("RegistrarReacao")
            .Produces<ReacoesResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest);

        grupo.MapDelete("/{tipo}", Remover)
            .AllowAnonymous()
            .RequireRateLimiting(RateLimiting.PolicyPublico)
            .WithName("RemoverReacao")
            .Produces<ReacoesResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest);

        return app;
    }

    private static async Task<IResult> Obter(
        Guid id,
        HttpContext contexto,
        ObterReacoesDoArtigoQueryHandler handler,
        CancellationToken ct)
    {
        var resultado = await handler.Handle(
            new ObterReacoesDoArtigoQuery(id, OrigemDoCliente.Visitante(contexto)), ct);

        return resultado.Sucesso ? Results.Ok(resultado.Valor) : ResultadoHttp.Falha(resultado.Erro);
    }

    private static async Task<IResult> Registrar(
        Guid id,
        RegistrarReacaoRequest requisicao,
        HttpContext contexto,
        RegistrarReacaoCommandHandler handler,
        CancellationToken ct)
    {
        if (!TipoReacaoValido(requisicao.Tipo, out var tipo))
        {
            return TipoInvalido();
        }

        var resultado = await handler.Handle(
            new RegistrarReacaoCommand(
                id,
                tipo,
                OrigemDoCliente.Visitante(contexto),
                OrigemDoCliente.Ip(contexto),
                OrigemDoCliente.UserAgent(contexto)),
            ct);

        return resultado.Sucesso ? Results.Ok(resultado.Valor) : ResultadoHttp.Falha(resultado.Erro);
    }

    private static async Task<IResult> Remover(
        Guid id,
        string tipo,
        HttpContext contexto,
        RemoverReacaoCommandHandler handler,
        CancellationToken ct)
    {
        if (!TipoReacaoValido(tipo, out var tipoReacao))
        {
            return TipoInvalido();
        }

        var resultado = await handler.Handle(
            new RemoverReacaoCommand(id, tipoReacao, OrigemDoCliente.Visitante(contexto)), ct);

        return resultado.Sucesso ? Results.Ok(resultado.Valor) : ResultadoHttp.Falha(resultado.Erro);
    }

    private static bool TipoReacaoValido(string? valor, out TipoReacao tipo) =>
        Enum.TryParse(valor, ignoreCase: true, out tipo) && Enum.IsDefined(tipo);

    private static IResult TipoInvalido() => Results.Problem(
        detail: "Tipo de reacao invalido.", statusCode: StatusCodes.Status400BadRequest, title: "Validacao");
}

/// <summary>Corpo do POST de reacao (nome do tipo, ex. "Curtir"; o id do artigo vem da rota).</summary>
public sealed record RegistrarReacaoRequest(string Tipo);
