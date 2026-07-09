namespace Levante.Identity.Infrastructure.Seguranca;

/// <summary>
/// Seed do admin. Email e SenhaInicial vem de user-secrets/env; vazio = nao semeia.
/// NUNCA commitar senha. Fora de Producao semeia automaticamente; em Producao exige o
/// opt-in explicito <see cref="PermitirSeedEmProducao"/> (usado pela stack conjunta na VM
/// para criar o primeiro admin a partir do .env).
/// </summary>
public sealed class AdminSeedOptions
{
    public const string SecaoConfig = "Admin";

    public string Email { get; set; } = string.Empty;

    public string SenhaInicial { get; set; } = string.Empty;

    /// <summary>
    /// Opt-in para semear o admin em Producao (default false). So tem efeito com Email/SenhaInicial
    /// preenchidos; o seed continua idempotente (semeia no maximo uma vez, se nao houver admin).
    /// </summary>
    public bool PermitirSeedEmProducao { get; set; }
}
