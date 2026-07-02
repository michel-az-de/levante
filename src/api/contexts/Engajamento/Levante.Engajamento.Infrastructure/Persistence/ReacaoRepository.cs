using Levante.Engajamento.Domain.Reacoes;
using MongoDB.Driver;

namespace Levante.Engajamento.Infrastructure.Persistence;

/// <summary>Repositorio do agregado Reacao (encapsula o MongoDB.Driver).</summary>
internal sealed class ReacaoRepository(EngajamentoMongoContext contexto) : IReacaoRepository
{
    private static readonly TipoReacao[] Tipos = Enum.GetValues<TipoReacao>();

    public async Task AddAsync(Reacao reacao, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(reacao);

        try
        {
            await contexto.Reacoes.InsertOneAsync(ReacaoDocument.DeDominio(reacao), options: null, ct);
        }
        catch (MongoWriteException ex) when (ex.WriteError?.Category == ServerErrorCategory.DuplicateKey)
        {
            // Indice unico violado: o visitante ja reagiu esse tipo. A Application trata como idempotente.
            throw new ReacaoDuplicadaException(reacao.ArtigoId, reacao.Tipo, reacao.Visitante);
        }
    }

    public Task RemoverAsync(Guid artigoId, TipoReacao tipo, string visitante, CancellationToken ct) =>
        contexto.Reacoes.DeleteOneAsync(
            d => d.ArtigoId == artigoId && d.Tipo == tipo && d.Visitante == visitante, ct);

    public async Task<IReadOnlyDictionary<TipoReacao, int>> ContarPorArtigoAsync(Guid artigoId, CancellationToken ct)
    {
        var contagens = new Dictionary<TipoReacao, int>(Tipos.Length);

        foreach (var tipo in Tipos)
        {
            var total = await contexto.Reacoes.CountDocumentsAsync(
                d => d.ArtigoId == artigoId && d.Tipo == tipo, options: null, ct);
            contagens[tipo] = (int)total;
        }

        return contagens;
    }

    public async Task<IReadOnlyList<TipoReacao>> ListarTiposDoVisitanteAsync(
        Guid artigoId, string visitante, CancellationToken ct)
    {
        var tipos = await contexto.Reacoes
            .Find(d => d.ArtigoId == artigoId && d.Visitante == visitante)
            .Project(d => d.Tipo)
            .ToListAsync(ct);

        return [.. tipos];
    }
}
