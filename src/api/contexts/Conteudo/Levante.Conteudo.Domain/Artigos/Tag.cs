using System.Text.RegularExpressions;

namespace Levante.Conteudo.Domain.Artigos;

/// <summary>
/// Value Object de tag (rotulo livre em kebab-case minusculo, criado implicitamente
/// ao digitar). Normaliza (trim + minusculas). Diferente do Slug de URL conceitualmente,
/// mas com a mesma regra de formato.
/// </summary>
public sealed partial record Tag
{
    public Tag(string valor)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(valor);

        var normalizado = valor.Trim().ToLowerInvariant();
        if (!Formato().IsMatch(normalizado))
        {
            throw new ArgumentException($"Tag invalida: '{valor}'. Use kebab-case (ex.: clean-architecture).", nameof(valor));
        }

        Valor = normalizado;
    }

    public string Valor { get; }

    public override string ToString() => Valor;

    /// <summary>Tenta criar uma Tag sem lancar (fluxo de validacao de entrada).</summary>
    public static bool TryParse(string? valor, out Tag? tag)
    {
        tag = null;
        if (string.IsNullOrWhiteSpace(valor))
        {
            return false;
        }

        var normalizado = valor.Trim().ToLowerInvariant();
        if (!Formato().IsMatch(normalizado))
        {
            return false;
        }

        tag = new Tag(normalizado);
        return true;
    }

    [GeneratedRegex("^[a-z0-9]+(?:-[a-z0-9]+)*$", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 250)]
    private static partial Regex Formato();
}
