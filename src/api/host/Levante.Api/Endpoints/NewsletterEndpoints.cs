using Levante.Api.Seguranca;
using Levante.Audiencia.Application.Assinantes.CancelarAssinatura;
using Levante.Audiencia.Application.Assinantes.ConfirmarAssinatura;
using Levante.Audiencia.Application.Assinantes.SolicitarAssinatura;

namespace Levante.Api.Endpoints;

/// <summary>
/// Endpoints publicos de newsletter (contexto Audiencia). Inscricao double opt-in:
/// solicitar (anonimo, honeypot, rate limit) -> e-mail de confirmacao (via Hiram) ->
/// confirmar/cancelar por token. Nada revela se um e-mail existe (privacidade/LGPD).
/// </summary>
public static class NewsletterEndpoints
{
    public static IEndpointRouteBuilder MapNewsletterEndpoints(this IEndpointRouteBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        var grupo = app.MapGroup("/newsletter").WithTags("Newsletter");

        grupo.MapPost("/", Solicitar)
            .AllowAnonymous()
            .RequireRateLimiting(RateLimiting.PolicyPublico)
            .WithName("SolicitarAssinatura")
            .Produces(StatusCodes.Status202Accepted)
            .ProducesProblem(StatusCodes.Status400BadRequest);

        grupo.MapPost("/confirmar", Confirmar)
            .AllowAnonymous()
            .RequireRateLimiting(RateLimiting.PolicyPublico)
            .WithName("ConfirmarAssinatura")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        grupo.MapPost("/cancelar", Cancelar)
            .AllowAnonymous()
            .RequireRateLimiting(RateLimiting.PolicyPublico)
            .WithName("CancelarAssinatura")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        return app;
    }

    private static async Task<IResult> Solicitar(
        SolicitarNewsletterRequest requisicao, SolicitarAssinaturaCommandHandler handler, CancellationToken ct)
    {
        var resultado = await handler.Handle(
            new SolicitarAssinaturaCommand(requisicao.Email, requisicao.Armadilha), ct);

        // 202: aceito para confirmacao (double opt-in). Sempre a mesma resposta (nao vaza existencia).
        return resultado.Sucesso ? Results.Accepted() : ResultadoHttp.Falha(resultado.Erro);
    }

    private static async Task<IResult> Confirmar(
        ConfirmarNewsletterRequest requisicao, ConfirmarAssinaturaCommandHandler handler, CancellationToken ct)
    {
        var resultado = await handler.Handle(new ConfirmarAssinaturaCommand(requisicao.Token), ct);
        return resultado.Sucesso ? Results.Ok() : ResultadoHttp.Falha(resultado.Erro);
    }

    private static async Task<IResult> Cancelar(
        CancelarNewsletterRequest requisicao, CancelarAssinaturaCommandHandler handler, CancellationToken ct)
    {
        var resultado = await handler.Handle(new CancelarAssinaturaCommand(requisicao.Token), ct);
        return resultado.Sucesso ? Results.Ok() : ResultadoHttp.Falha(resultado.Erro);
    }
}

/// <summary>Corpo do POST de inscricao. <c>Armadilha</c> e o honeypot (deve vir vazio).</summary>
public sealed record SolicitarNewsletterRequest(string Email, string? Armadilha = null);

/// <summary>Corpo do POST de confirmacao (token do link enviado por e-mail).</summary>
public sealed record ConfirmarNewsletterRequest(string Token);

/// <summary>Corpo do POST de cancelamento (token de descadastro).</summary>
public sealed record CancelarNewsletterRequest(string Token);
