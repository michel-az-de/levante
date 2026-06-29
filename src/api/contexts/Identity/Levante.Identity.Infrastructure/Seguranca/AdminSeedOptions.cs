namespace Levante.Identity.Infrastructure.Seguranca;

/// <summary>
/// Seed do admin (Dev/testes). Email e SenhaInicial vem de user-secrets/env;
/// vazio = nao semeia. NUNCA commitar senha.
/// </summary>
public sealed class AdminSeedOptions
{
    public const string SecaoConfig = "Admin";

    public string Email { get; set; } = string.Empty;

    public string SenhaInicial { get; set; } = string.Empty;
}
