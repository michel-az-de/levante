using System.Security.Claims;
using Levante.Api.Seguranca;
using Levante.Identity.Application.Autenticacao;

namespace Levante.Api.Endpoints;

/// <summary>Endpoints de autenticacao do admin (JWT bearer).</summary>
public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        var grupo = app.MapGroup("/auth").WithTags("Auth");

        grupo.MapPost("/login", Login)
            .AllowAnonymous() // publico (login); rate limit estrito
            .RequireRateLimiting(RateLimiting.PolicyAuth)
            .WithName("Autenticar")
            .Produces<TokenDeAcessoResponse>()
            .Produces(StatusCodes.Status401Unauthorized);

        grupo.MapGet("/eu", Eu)
            .RequireAuthorization() // protegido: prova o esquema JWT + deny-by-default
            .WithName("ObterAdministradorAtual")
            .Produces<AdministradorAtualResponse>();

        return app;
    }

    private static async Task<IResult> Login(
        AutenticarRequest requisicao,
        AutenticarCommandHandler handler,
        CancellationToken ct)
    {
        var resultado = await handler.Handle(new AutenticarCommand(requisicao.Email, requisicao.Senha), ct);

        // 401 para qualquer falha (credenciais ou bloqueio): nao vaza detalhe.
        return resultado.Sucesso ? Results.Ok(resultado.Valor) : Results.Unauthorized();
    }

    private static IResult Eu(ClaimsPrincipal usuario)
    {
        var email = usuario.FindFirstValue(ClaimTypes.Email)
            ?? usuario.FindFirstValue("email")
            ?? string.Empty;
        return Results.Ok(new AdministradorAtualResponse(email));
    }
}

/// <summary>Corpo do login.</summary>
public sealed record AutenticarRequest(string Email, string Senha);

/// <summary>Resposta de /auth/eu (admin autenticado).</summary>
public sealed record AdministradorAtualResponse(string Email);
