namespace Levante.Conteudo.Domain.Artigos;

/// <summary>
/// Porta de persistencia do agregado Artigo. Contrato no dominio (noun PT +
/// Repository); metodos em EN (classe tecnica). Implementacao na Infrastructure.
/// </summary>
public interface IArtigoRepository
{
    Task<IReadOnlyList<Artigo>> ListPublicadosAsync(CancellationToken ct);

    Task<Artigo?> GetBySlugAsync(string slug, CancellationToken ct);

    Task AddAsync(Artigo artigo, CancellationToken ct);
}
