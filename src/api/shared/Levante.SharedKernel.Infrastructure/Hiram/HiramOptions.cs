namespace Levante.SharedKernel.Infrastructure.Hiram;

/// <summary>
/// Conexao com o Hiram (destino do relay do Outbox, HTTP <c>POST /v1/events</c>).
/// A <see cref="ApiKey"/> (tenant) vem de user-secrets (dev) e env / Key Vault (prod),
/// NUNCA do repositorio. Bind lazy (sem ValidateOnStart): quando o relay esta
/// desabilitado nada e exigido; habilitado sem Hiram acessivel, o relay falha ao
/// publicar e reprocessa por backoff (logado), em vez de derrubar o boot.
/// </summary>
public sealed class HiramOptions
{
    public const string SecaoConfig = "Hiram";

    /// <summary>URL base do Hiram (ex. <c>http://hiram-api:8080</c>).</summary>
    public Uri? BaseUrl { get; set; }

    /// <summary>API key do tenant Levante (<c>hk_live_...</c>); header <c>X-Api-Key</c>.</summary>
    public string ApiKey { get; set; } = string.Empty;
}
