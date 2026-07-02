using Levante.Engajamento.Domain.Comentarios;
using Levante.SharedKernel.Infrastructure.Outbox;
using MongoDB.Driver;

namespace Levante.Engajamento.Infrastructure.Persistence;

/// <summary>
/// Repositorio do agregado Comentario. As escritas passam pelo
/// <see cref="IGravadorDeAgregado"/>: os eventos (ComentarioCriado/ComentarioAprovado)
/// vao ao Outbox na mesma transacao da gravacao.
/// </summary>
internal sealed class ComentarioRepository(EngajamentoMongoContext contexto, IGravadorDeAgregado gravador)
    : IComentarioRepository
{
    public async Task AddAsync(Comentario comentario, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(comentario);

        var documento = ComentarioDocument.DeDominio(comentario);
        await gravador.ExecutarAsync(
            (sessao, c) => sessao is null
                ? contexto.Comentarios.InsertOneAsync(documento, options: null, c)
                : contexto.Comentarios.InsertOneAsync(sessao, documento, options: null, c),
            comentario.Eventos,
            ct);

        comentario.LimparEventos();
    }

    public async Task<Comentario?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        var doc = await contexto.Comentarios.Find(d => d.Id == id).FirstOrDefaultAsync(ct);
        return doc?.ParaDominio();
    }

    public async Task UpdateAsync(Comentario comentario, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(comentario);

        var documento = ComentarioDocument.DeDominio(comentario);
        await gravador.ExecutarAsync(
            (sessao, c) => sessao is null
                ? contexto.Comentarios.ReplaceOneAsync(d => d.Id == documento.Id, documento, new ReplaceOptions(), c)
                : contexto.Comentarios.ReplaceOneAsync(sessao, d => d.Id == documento.Id, documento, new ReplaceOptions(), c),
            comentario.Eventos,
            ct);

        comentario.LimparEventos();
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
