using System.ComponentModel.DataAnnotations;

namespace Levante.SharedKernel.Infrastructure.Outbox;

/// <summary>
/// Conexao do RabbitMQ (destino do relay do Outbox). Credenciais via user-secrets
/// (dev) e env / Key Vault (prod), NUNCA do repositorio. Validado no boot so quando
/// o relay esta habilitado.
/// </summary>
public sealed class RabbitMqOptions
{
    public const string SecaoConfig = "RabbitMq";

    [Required]
    public string Hostname { get; set; } = string.Empty;

    [Range(1, 65535)]
    public int Port { get; set; } = 5672;

    [Required]
    public string Username { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;

    [Required]
    public string VirtualHost { get; set; } = "/";

    /// <summary>Exchange topic (duravel) onde os eventos sao publicados; routing key = nome do evento.</summary>
    [Required]
    public string Exchange { get; set; } = "levante.eventos";
}
