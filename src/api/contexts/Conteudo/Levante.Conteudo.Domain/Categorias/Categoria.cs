using Levante.Conteudo.Domain.Artigos;

namespace Levante.Conteudo.Domain.Categorias;

/// <summary>
/// Agregado de categoria de conteudo (taxonomia curada; 1 por artigo, opcional).
/// O <see cref="Slug"/> e imutavel apos a criacao (evita quebrar URLs e referencias).
/// Reusa o VO <see cref="Slug"/> do contexto Conteudo.
/// </summary>
public sealed class Categoria
{
    public const int TamanhoMaximoNome = 80;
    public const int TamanhoMaximoDescricao = 280;

    private Categoria(Guid id, string nome, Slug slug, string? descricao, DateTime dataCriacao)
    {
        Id = id;
        Nome = nome;
        Slug = slug;
        Descricao = descricao;
        DataCriacao = dataCriacao;
    }

    public Guid Id { get; }

    public string Nome { get; private set; }

    public Slug Slug { get; }

    public string? Descricao { get; private set; }

    public DateTime DataCriacao { get; }

    /// <summary>Cria uma nova categoria (slug informado pelo admin, imutavel daqui em diante).</summary>
    public static Categoria Criar(string nome, Slug slug, string? descricao = null)
    {
        GarantirNome(nome);
        GarantirDescricao(descricao);
        ArgumentNullException.ThrowIfNull(slug);

        return new Categoria(Guid.NewGuid(), nome.Trim(), slug, Normalizar(descricao), DateTime.UtcNow);
    }

    /// <summary>Rehidrata uma categoria existente (uso da camada de persistencia).</summary>
    public static Categoria Reconstituir(Guid id, string nome, Slug slug, string? descricao, DateTime dataCriacao) =>
        new(id, nome, slug, descricao, dataCriacao);

    /// <summary>Atualiza nome e descricao (o slug permanece imutavel).</summary>
    public void Editar(string nome, string? descricao)
    {
        GarantirNome(nome);
        GarantirDescricao(descricao);

        Nome = nome.Trim();
        Descricao = Normalizar(descricao);
    }

    private static void GarantirNome(string nome)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nome);

        if (nome.Trim().Length > TamanhoMaximoNome)
        {
            throw new ArgumentException($"Nome excede {TamanhoMaximoNome} caracteres.", nameof(nome));
        }
    }

    private static void GarantirDescricao(string? descricao)
    {
        if (descricao is { Length: > TamanhoMaximoDescricao })
        {
            throw new ArgumentException($"Descricao excede {TamanhoMaximoDescricao} caracteres.", nameof(descricao));
        }
    }

    private static string? Normalizar(string? valor) =>
        string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
}
