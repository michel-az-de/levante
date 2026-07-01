using System.ComponentModel.DataAnnotations;

namespace Levante.SharedKernel.Infrastructure;

/// <summary>
/// Conexao do Mongo compartilhada pelos contextos (mesmo cluster/database;
/// cada contexto tem as SUAS collections). Valores vem de user-secrets (dev)
/// e variaveis de ambiente / Key Vault (prod). NUNCA do repositorio.
/// </summary>
public sealed class MongoOptions
{
    public const string SecaoConfig = "Mongo";

    [Required]
    public string ConnectionString { get; set; } = string.Empty;

    [Required]
    public string DatabaseName { get; set; } = "levante";
}
