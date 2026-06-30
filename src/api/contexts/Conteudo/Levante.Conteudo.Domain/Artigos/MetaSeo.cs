namespace Levante.Conteudo.Domain.Artigos;

/// <summary>
/// Value Object de metadados de SEO editaveis por artigo (override do title/description/OG).
/// Todos opcionais; quando ausentes, o publico cai no Titulo/Resumo/OG dinamico (fallback).
/// Strings vazias/em-branco viram null (normalizacao). Guard de tamanho alinhado ao SEO.
/// </summary>
public sealed record MetaSeo
{
    /// <summary>Limite recomendado do title em SERPs.</summary>
    public const int TamanhoMaximoTitulo = 60;

    /// <summary>Limite recomendado da meta description em SERPs.</summary>
    public const int TamanhoMaximoDescricao = 155;

    /// <summary>Meta SEO sem nenhum override (estado padrao do artigo).</summary>
    public static readonly MetaSeo Vazio = new(null, null, null);

    private MetaSeo(string? titulo, string? descricao, string? imagemOgUrl)
    {
        Titulo = titulo;
        Descricao = descricao;
        ImagemOgUrl = imagemOgUrl;
    }

    public string? Titulo { get; }

    public string? Descricao { get; }

    public string? ImagemOgUrl { get; }

    /// <summary>Cria o VO normalizando vazios para null e validando os limites de tamanho.</summary>
    public static MetaSeo Criar(string? titulo, string? descricao, string? imagemOgUrl)
    {
        var tituloNormalizado = Normalizar(titulo);
        var descricaoNormalizada = Normalizar(descricao);

        if (tituloNormalizado is { Length: > TamanhoMaximoTitulo })
        {
            throw new ArgumentException(
                $"Meta titulo excede {TamanhoMaximoTitulo} caracteres.", nameof(titulo));
        }

        if (descricaoNormalizada is { Length: > TamanhoMaximoDescricao })
        {
            throw new ArgumentException(
                $"Meta descricao excede {TamanhoMaximoDescricao} caracteres.", nameof(descricao));
        }

        return new MetaSeo(tituloNormalizado, descricaoNormalizada, Normalizar(imagemOgUrl));
    }

    private static string? Normalizar(string? valor) =>
        string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
}
