using Levante.Engajamento.Domain.Comentarios;
using MongoDB.Driver;

namespace Levante.Engajamento.Infrastructure.Persistence;

/// <summary>Repositorio do agregado Comentario (encapsula o MongoDB.Driver).</summary>
internal sealed class ComentarioRepository(EngajamentoMongoContext contexto) : IComentarioRepository
{
    public Task AddAsync(Comentario comentario, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(comentario);
        return contexto.Comentarios.InsertOneAsync(ComentarioDocument.DeDominio(comentario), options: null, ct);
    }

    public async Task<Comentario?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        var doc = await contexto.Comentarios.Find(d => d.Id == id).FirstOrDefaultAsync(ct);
        return doc?.ParaDominio();
    }

    public Task UpdateAsync(Comentario comentario, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(comentario);
        return contexto.Comentarios.ReplaceOneAsync(
            d => d.Id == comentario.Id, ComentarioDocument.DeDominio(comentario), new ReplaceOptions(), ct);
    }

    public async Task<IReadOnlyList<Comentario>> ListarAprovadosPorArtigoAsync(Guid artigoId, CancellationToken ct)
    {
        var docs = await contexto.Comentarios
            .Find(d => d.ArtigoId == artigoId && d.Status == StatusComentario.Aprovado)
            .SortBy(d => d.DataCriacao)
            .ToListAsync(ct);

        return [.. docs.Select(d => d.ParaDominio())];
    }

    public async Task<IReadOnlyList<Comentario>> ListarPendentesAsync(CancellationToken ct)
    {
        var docs = await contexto.Comentarios
            .Find(d => d.Status == StatusComentario.Pendente)
            .SortBy(d => d.DataCriacao)
            .ToListAsync(ct);

        return [.. docs.Select(d => d.ParaDominio())];
    }
}
