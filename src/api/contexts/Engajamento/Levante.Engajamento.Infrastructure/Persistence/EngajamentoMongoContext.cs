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
        Comentarios = Database.GetCollection<ComentarioDocument>("comentarios");
    }

    public IMongoDatabase Database { get; }

    public IMongoCollection<ReacaoDocument> Reacoes { get; }

    public IMongoCollection<ComentarioDocument> Comentarios { get; }

    /// <summary>
    /// Indices do contexto: reacao unica por (artigo, visitante, tipo) e um indice
    /// de leitura dos comentarios por (artigo, status). Idempotente; seguro sob scale-out.
    /// </summary>
    public async Task EnsureIndexesAsync(CancellationToken ct)
    {
        var indiceReacao = new CreateIndexModel<ReacaoDocument>(
            Builders<ReacaoDocument>.IndexKeys
                .Ascending(d => d.ArtigoId)
                .Ascending(d => d.Visitante)
                .Ascending(d => d.Tipo),
            new CreateIndexOptions { Unique = true, Name = "ux_reacoes_artigo_visitante_tipo" });

        var indiceComentario = new CreateIndexModel<ComentarioDocument>(
            Builders<ComentarioDocument>.IndexKeys
                .Ascending(d => d.ArtigoId)
                .Ascending(d => d.Status),
            new CreateIndexOptions { Name = "ix_comentarios_artigo_status" });

        await Reacoes.Indexes.CreateOneAsync(indiceReacao, cancellationToken: ct);
        await Comentarios.Indexes.CreateOneAsync(indiceComentario, cancellationToken: ct);
    }
}
