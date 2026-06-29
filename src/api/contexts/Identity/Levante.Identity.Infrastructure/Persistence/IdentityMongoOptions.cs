using System.ComponentModel.DataAnnotations;

namespace Levante.Identity.Infrastructure.Persistence;

/// <summary>
/// Conexao do Mongo para o contexto Identity (mesma seca "Mongo" do Conteudo;
/// compartilha o cluster/database). Sem secrets no repo. TODO: consolidar num
/// modulo de infra compartilhado entre contextos.
/// </summary>
public sealed class IdentityMongoOptions
{
    public const string SecaoConfig = "Mongo";

    [Required]
    public string ConnectionString { get; set; } = string.Empty;

    [Required]
    public string DatabaseName { get; set; } = "levante";
}
