using System.ComponentModel.DataAnnotations;

namespace Levante.Conteudo.Infrastructure.Persistence;

/// <summary>
/// Opcoes de conexao do Mongo. Valores vem de user-secrets (dev) e
/// variaveis de ambiente / Key Vault (prod). NUNCA do repositorio.
/// </summary>
public sealed class ConteudoMongoOptions
{
    public const string SecaoConfig = "Mongo";

    [Required]
    public string ConnectionString { get; set; } = string.Empty;

    [Required]
    public string DatabaseName { get; set; } = "levante";
}
