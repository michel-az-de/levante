namespace Levante.Api.Seguranca;

/// <summary>
/// Headers de seguranca padrao (nao opcionais, ver CLAUDE.md). Minimo na
/// Fatia 0; hardening completo (CSP por rota, etc.) vem depois.
/// </summary>
public static class SecurityHeaders
{
    public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        return app.Use(static async (context, next) =>
        {
            var headers = context.Response.Headers;
            headers["X-Content-Type-Options"] = "nosniff";
            headers["X-Frame-Options"] = "DENY";
            headers["Referrer-Policy"] = "no-referrer";
            headers["Content-Security-Policy"] = "default-src 'none'; frame-ancestors 'none'";
            await next();
        });
    }
}
