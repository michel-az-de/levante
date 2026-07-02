using System.ComponentModel.DataAnnotations;

namespace Levante.Engajamento.Infrastructure.Seguranca;

/// <summary>
/// Opcoes do contexto Engajamento. <see cref="OrigemHashSecret"/> e o segredo do
/// HMAC que pseudonimiza IP+User-Agent (dedup/anti-abuso). Vem de user-secrets
/// (dev) e env / Key Vault (prod), NUNCA do repositorio. ValidateOnStart garante
/// presenca no boot real.
/// </summary>
public sealed class EngajamentoOptions
{
    public const string SecaoConfig = "Engajamento";

    [Required]
    [MinLength(32)]
    public string OrigemHashSecret { get; set; } = string.Empty;
}
