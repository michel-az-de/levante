using Levante.Engajamento.Domain.Comentarios;

namespace Levante.Engajamento.UnitTests.Comentarios;

/// <summary>Fake em memoria de <see cref="IComentarioRepository"/> com contadores.</summary>
internal sealed class ComentarioRepositorioEmMemoria : IComentarioRepository
{
    private readonly List<Comentario> _comentarios;

    public ComentarioRepositorioEmMemoria(params Comentario[] comentarios) => _comentarios = [.. comentarios];

    public int Adicionados { get; private set; }

    public int Atualizados { get; private set; }

    public Task AddAsync(Comentario comentario, CancellationToken ct)
    {
        _comentarios.Add(comentario);
        Adicionados++;
        return Task.CompletedTask;
    }

    public Task<Comentario?> GetByIdAsync(Guid id, CancellationToken ct) =>
        Task.FromResult(_comentarios.FirstOrDefault(c => c.Id == id));

    public Task UpdateAsync(Comentario comentario, CancellationToken ct)
    {
        Atualizados++;
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<Comentario>> ListarAprovadosPorArtigoAsync(Guid artigoId, CancellationToken ct)
    {
        IReadOnlyList<Comentario> aprovados =
        [
            .. _comentarios.Where(c => c.ArtigoId == artigoId && c.Status == StatusComentario.Aprovado),
        ];
        return Task.FromResult(aprovados);
    }

    public Task<IReadOnlyList<Comentario>> ListarPendentesAsync(CancellationToken ct)
    {
        IReadOnlyList<Comentario> pendentes = [.. _comentarios.Where(c => c.Status == StatusComentario.Pendente)];
        return Task.FromResult(pendentes);
    }
}
