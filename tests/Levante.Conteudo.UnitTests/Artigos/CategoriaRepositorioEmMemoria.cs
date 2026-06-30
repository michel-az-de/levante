using Levante.Conteudo.Domain.Categorias;

namespace Levante.Conteudo.UnitTests.Artigos;

/// <summary>Repositorio de categorias em memoria para testar handlers/validators sem Mongo.</summary>
internal sealed class CategoriaRepositorioEmMemoria : ICategoriaRepository
{
    private readonly List<Categoria> _categorias;

    public CategoriaRepositorioEmMemoria(params Categoria[] categorias) => _categorias = [.. categorias];

    public int Adicionadas { get; private set; }

    public int Atualizadas { get; private set; }

    public Task<IReadOnlyList<Categoria>> ListAsync(CancellationToken ct) =>
        Task.FromResult<IReadOnlyList<Categoria>>([.. _categorias]);

    public Task<Categoria?> GetByIdAsync(Guid id, CancellationToken ct) =>
        Task.FromResult(_categorias.FirstOrDefault(c => c.Id == id));

    public Task<Categoria?> GetBySlugAsync(string slug, CancellationToken ct) =>
        Task.FromResult(_categorias.FirstOrDefault(c => c.Slug.Valor == slug));

    public Task AddAsync(Categoria categoria, CancellationToken ct)
    {
        _categorias.Add(categoria);
        Adicionadas++;
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Categoria categoria, CancellationToken ct)
    {
        Atualizadas++;
        return Task.CompletedTask;
    }
}
