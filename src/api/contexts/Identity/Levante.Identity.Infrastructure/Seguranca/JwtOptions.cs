using System.ComponentModel.DataAnnotations;

namespace Levante.Identity.Infrastructure.Seguranca;

/// <summary>
/// Opcoes do JWT. SecretKey vem de user-secrets (dev) e env / Key Vault (prod),
/// NUNCA do repositorio. ValidateOnStart garante presenca no boot real.
/// </summary>
public sealed class JwtOptions
{
    public const string SecaoConfig = "Jwt";

    [Required]
    public string Issuer { get; set; } = "levante";

    [Required]
    public string Audience { get; set; } = "levante";

    [Required]
    [MinLength(32)]
    public string SecretKey { get; set; } = string.Empty;

    [Range(1, 1440)]
    public int ExpiraEmMinutos { get; set; } = 60;
}
