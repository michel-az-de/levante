namespace Levante.Conteudo.Domain.Categorias;

/// <summary>
/// Porta de persistencia do agregado Categoria. Contrato no dominio (noun PT +
/// Repository); metodos em EN. Implementacao na Infrastructure.
/// </summary>
public interface ICategoriaRepository
{
    Task<IReadOnlyList<Categoria>> ListAsync(CancellationToken ct);

    Task<Categoria?> GetByIdAsync(Guid id, CancellationToken ct);

    Task<Categoria?> GetBySlugAsync(string slug, CancellationToken ct);

    Task AddAsync(Categoria categoria, CancellationToken ct);

    Task UpdateAsync(Categoria categoria, CancellationToken ct);
}
