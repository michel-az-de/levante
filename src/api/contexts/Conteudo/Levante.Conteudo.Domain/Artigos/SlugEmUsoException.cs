namespace Levante.Conteudo.Domain.Artigos;

/// <summary>
/// Slug ja em uso (violacao do indice unico de slug). A Infrastructure traduz o
/// erro de chave duplicada do Mongo nesta excecao; a Application a converte em
/// Result ("slug_em_uso" -> 409). Rede de seguranca da corrida: o caminho comum
/// e a pre-checagem por Result, esta excecao cobre o TOCTOU concorrente.
/// </summary>
public sealed class SlugEmUsoException(string slug)
    : Exception($"Ja existe um artigo com o slug '{slug}'.")
{
    public string Slug { get; } = slug;
}
