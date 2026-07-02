using Levante.SharedKernel.Infrastructure;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Levante.Engajamento.Infrastructure.Persistence;

/// <summary>Acesso tipado ao banco do contexto Engajamento (singleton; compartilha o cluster).</summary>
internal sealed class EngajamentoMongoContext
{
    public EngajamentoMongoContext(IMongoClient client, IOptions<MongoOptions> options)
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentNullException.ThrowIfNull(options);

        Database = client.GetDatabase(options.Value.DatabaseName);
        Reacoes = Database.GetCollection<ReacaoDocument>("reacoes");
    }

    public IMongoDatabase Database { get; }

    public IMongoCollection<ReacaoDocument> Reacoes { get; }

    /// <summary>
    /// Indice unico da reacao por (artigo, visitante, tipo): garante uma reacao
    /// de cada tipo por visitante/artigo. Idempotente; seguro sob scale-out.
    /// </summary>
    public Task EnsureIndexesAsync(CancellationToken ct)
    {
        var indice = new CreateIndexModel<ReacaoDocument>(
            Builders<ReacaoDocument>.IndexKeys
                .Ascending(d => d.ArtigoId)
                .Ascending(d => d.Visitante)
                .Ascending(d => d.Tipo),
            new CreateIndexOptions { Unique = true, Name = "ux_reacoes_artigo_visitante_tipo" });

        return Reacoes.Indexes.CreateOneAsync(indice, cancellationToken: ct);
    }
}
