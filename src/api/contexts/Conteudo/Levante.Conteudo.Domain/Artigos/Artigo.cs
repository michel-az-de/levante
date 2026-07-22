using Levante.SharedKernel;

namespace Levante.Conteudo.Domain.Artigos;

/// <summary>
/// Agregado de artigo. Estado mutado apenas por comportamentos (verbos PT).
/// Persistencia mapeia via documento na Infrastructure; o agregado e puro.
/// </summary>
public sealed class Artigo
{
    private readonly List<IEventoDeDominio> _eventos = [];

    /// <summary>Tamanho maximo do resumo (alinhado a meta description de SEO).</summary>
    public const int TamanhoMaximoResumo = 280;

    /// <summary>
    /// Tamanho maximo do conteudo (corpo markdown). Teto generoso: defende o documento
    /// Mongo (limite de 16MB) sem atrapalhar artigos longos. Decidido na issue #101.
    /// </summary>
    public const int TamanhoMaximoConteudo = 200_000;

    private Artigo(
        Guid id,
        string titulo,
        Slug slug,
        string resumo,
        string conteudo,
        StatusArtigo status,
        DateTime dataCriacao,
        DateTime? dataPublicacao,
        MetaSeo meta,
        Guid? categoriaId,
        IReadOnlyList<Tag> tags)
    {
        Id = id;
        Titulo = titulo;
        Slug = slug;
        Resumo = resumo;
        Conteudo = conteudo;
        Status = status;
        DataCriacao = dataCriacao;
        DataPublicacao = dataPublicacao;
        Meta = meta;
        CategoriaId = categoriaId;
        Tags = tags;
    }

    public Guid Id { get; }

    public string Titulo { get; private set; }

    public Slug Slug { get; private set; }

    /// <summary>Resumo curto (vira a meta description / descricao OG no front).</summary>
    public string Resumo { get; private set; }

    public string Conteudo { get; private set; }

    public StatusArtigo Status { get; private set; }

    public DateTime DataCriacao { get; }

    public DateTime? DataPublicacao { get; private set; }

    /// <summary>Metadados de SEO editaveis (override de title/description/OG). Nunca null; Vazio por padrao.</summary>
    public MetaSeo Meta { get; private set; }

    /// <summary>Categoria associada (taxonomia curada; opcional, no maximo uma).</summary>
    public Guid? CategoriaId { get; private set; }

    /// <summary>Tags livres (rotulos kebab-case). Nunca null; lista vazia por padrao.</summary>
    public IReadOnlyList<Tag> Tags { get; private set; }

    public IReadOnlyList<IEventoDeDominio> Eventos => _eventos;

    /// <summary>Cria um novo artigo em rascunho. <paramref name="meta"/> ausente vira <see cref="MetaSeo.Vazio"/>.</summary>
    public static Artigo Criar(
        string titulo,
        Slug slug,
        string resumo,
        string conteudo,
        MetaSeo? meta = null,
        Guid? categoriaId = null,
        IReadOnlyList<Tag>? tags = null)
    {
        GarantirCampos(titulo, slug, resumo, conteudo);

        return new Artigo(
            Guid.NewGuid(),
            titulo,
            slug,
            resumo,
            conteudo,
            StatusArtigo.Rascunho,
            DateTime.UtcNow,
            dataPublicacao: null,
            meta ?? MetaSeo.Vazio,
            categoriaId,
            tags ?? []);
    }

    /// <summary>Rehidrata um artigo existente (uso da camada de persistencia).</summary>
    public static Artigo Reconstituir(
        Guid id,
        string titulo,
        Slug slug,
        string resumo,
        string conteudo,
        StatusArtigo status,
        DateTime dataCriacao,
        DateTime? dataPublicacao,
        MetaSeo? meta = null,
        Guid? categoriaId = null,
        IReadOnlyList<Tag>? tags = null) =>
        new(id, titulo, slug, resumo, conteudo, status, dataCriacao, dataPublicacao, meta ?? MetaSeo.Vazio, categoriaId, tags ?? []);

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

    /// <summary>Atualiza os campos editaveis (titulo, slug, resumo, conteudo, meta SEO, categoria e tags).</summary>
    public void Editar(
        string titulo,
        Slug slug,
        string resumo,
        string conteudo,
        MetaSeo? meta = null,
        Guid? categoriaId = null,
        IReadOnlyList<Tag>? tags = null)
    {
        GarantirCampos(titulo, slug, resumo, conteudo);

        Titulo = titulo;
        Slug = slug;
        Resumo = resumo;
        Conteudo = conteudo;
        Meta = meta ?? MetaSeo.Vazio;
        CategoriaId = categoriaId;
        Tags = tags ?? [];
    }

    /// <summary>Arquiva o artigo (estado terminal; idempotente). A acao "despublicar".</summary>
    public void Arquivar()
    {
        if (Status == StatusArtigo.Arquivado)
        {
            return;
        }

        Status = StatusArtigo.Arquivado;
    }

    /// <summary>Limpa eventos ja despachados (chamado apos persistir no Outbox).</summary>
    public void LimparEventos() => _eventos.Clear();

    private static void GarantirCampos(string titulo, Slug slug, string resumo, string conteudo)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(titulo);
        ArgumentNullException.ThrowIfNull(slug);
        ArgumentException.ThrowIfNullOrWhiteSpace(resumo);
        ArgumentException.ThrowIfNullOrWhiteSpace(conteudo);

        if (resumo.Length > TamanhoMaximoResumo)
        {
            throw new ArgumentException(
                $"Resumo excede {TamanhoMaximoResumo} caracteres.", nameof(resumo));
        }

        if (conteudo.Length > TamanhoMaximoConteudo)
        {
            throw new ArgumentException(
                $"Conteudo excede {TamanhoMaximoConteudo} caracteres.", nameof(conteudo));
        }
    }
}
