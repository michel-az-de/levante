using System.Text.RegularExpressions;

namespace Levante.Conteudo.Domain.Artigos;

/// <summary>
/// Value Object de slug em kebab-case (usado em URLs PT: /artigos/[slug]).
/// Invariante protegida por guard clause (ver CLAUDE.md, Estilo de codigo).
/// </summary>
public sealed partial record Slug
{
    public Slug(string valor)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(valor);

        if (!FormatoKebabCase().IsMatch(valor))
        {
            throw new ArgumentException(
                $"Slug invalido: '{valor}'. Use kebab-case (ex.: clean-architecture-na-pratica).",
                nameof(valor));
        }

        Valor = valor;
    }

    public string Valor { get; }

    public override string ToString() => Valor;

    /// <summary>Tenta criar um Slug sem lancar (fluxo de negocio esperado, ex.: validacao de comando).</summary>
    public static bool TryParse(string? valor, out Slug? slug)
    {
        slug = null;
        if (string.IsNullOrWhiteSpace(valor) || !FormatoKebabCase().IsMatch(valor))
        {
            return false;
        }

        slug = new Slug(valor);
        return true;
    }

    [GeneratedRegex("^[a-z0-9]+(?:-[a-z0-9]+)*$", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 250)]
    private static partial Regex FormatoKebabCase();
}
