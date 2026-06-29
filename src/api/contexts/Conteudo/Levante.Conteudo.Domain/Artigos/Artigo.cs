using Levante.SharedKernel;

namespace Levante.Conteudo.Domain.Artigos;

/// <summary>
/// Agregado de artigo. Estado mutado apenas por comportamentos (verbos PT).
/// Persistencia mapeia via documento na Infrastructure; o agregado e puro.
/// </summary>
public sealed class Artigo
{
    private readonly List<IEventoDeDominio> _eventos = [];

    private Artigo(
        Guid id,
        string titulo,
        Slug slug,
        string conteudo,
        StatusArtigo status,
        DateTime dataCriacao,
        DateTime? dataPublicacao)
    {
        Id = id;
        Titulo = titulo;
        Slug = slug;
        Conteudo = conteudo;
        Status = status;
        DataCriacao = dataCriacao;
        DataPublicacao = dataPublicacao;
    }

    public Guid Id { get; }

    public string Titulo { get; private set; }

    public Slug Slug { get; private set; }

    public string Conteudo { get; private set; }

    public StatusArtigo Status { get; private set; }

    public DateTime DataCriacao { get; }

    public DateTime? DataPublicacao { get; private set; }

    public IReadOnlyList<IEventoDeDominio> Eventos => _eventos;

    /// <summary>Cria um novo artigo em rascunho.</summary>
    public static Artigo Criar(string titulo, Slug slug, string conteudo)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(titulo);
        ArgumentNullException.ThrowIfNull(slug);
        ArgumentException.ThrowIfNullOrWhiteSpace(conteudo);

        return new Artigo(
            Guid.NewGuid(),
            titulo,
            slug,
            conteudo,
            StatusArtigo.Rascunho,
            DateTime.UtcNow,
            dataPublicacao: null);
    }

    /// <summary>Rehidrata um artigo existente (uso da camada de persistencia).</summary>
    public static Artigo Reconstituir(
        Guid id,
        string titulo,
        Slug slug,
        string conteudo,
        StatusArtigo status,
        DateTime dataCriacao,
        DateTime? dataPublicacao) =>
        new(id, titulo, slug, conteudo, status, dataCriacao, dataPublicacao);

    /// <summary>Publica o artigo (idempotente) e registra o evento de dominio.</summary>
    public void Publicar()
    {
        if (Status == StatusArtigo.Publicado)
        {
            return;
        }

        Status = StatusArtigo.Publicado;
        DataPublicacao = DateTime.UtcNow;
        _eventos.Add(new ArtigoPublicado(Id, Slug.Valor, DataPublicacao.Value));
    }

    /// <summary>Limpa eventos ja despachados (chamado apos persistir no Outbox).</summary>
    public void LimparEventos() => _eventos.Clear();
}
