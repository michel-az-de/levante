using Levante.Conteudo.Domain.Artigos;

namespace Levante.Conteudo.UnitTests.Artigos;

/// <summary>
/// Repositorio de artigos em memoria para testar handlers sem Mongo (unit).
/// A integracao real (ReplaceOne/indice unico) e coberta nos testes de integracao.
/// </summary>
internal sealed class ArtigoRepositorioEmMemoria : IArtigoRepository
{
    private readonly List<Artigo> _artigos;

    public ArtigoRepositorioEmMemoria(params Artigo[] artigos) => _artigos = [.. artigos];

    public int Adicionados { get; private set; }

    public int Atualizados { get; private set; }

    public Task<IReadOnlyList<Artigo>> ListPublicadosAsync(CancellationToken ct) =>
        Task.FromResult<IReadOnlyList<Artigo>>(
            [.. _artigos.Where(a => a.Status == StatusArtigo.Publicado)]);

    public Task<IReadOnlyList<Artigo>> ListTodosAsync(CancellationToken ct) =>
        Task.FromResult<IReadOnlyList<Artigo>>([.. _artigos]);

    public Task<Artigo?> GetBySlugAsync(string slug, CancellationToken ct) =>
        Task.FromResult(_artigos.FirstOrDefault(a => a.Slug.Valor == slug));

    public Task<Artigo?> GetByIdAsync(Guid id, CancellationToken ct) =>
        Task.FromResult(_artigos.FirstOrDefault(a => a.Id == id));

    public Task AddAsync(Artigo artigo, CancellationToken ct)
    {
        _artigos.Add(artigo);
        Adicionados++;
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Artigo artigo, CancellationToken ct)
    {
        Atualizados++;
        return Task.CompletedTask;
    }
}
