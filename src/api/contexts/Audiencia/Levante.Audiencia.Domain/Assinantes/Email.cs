using System.Text.RegularExpressions;

namespace Levante.Audiencia.Domain.Assinantes;

/// <summary>Value Object de e-mail (normalizado em minusculas). Guard clause de formato.</summary>
public sealed partial record Email
{
    public Email(string valor)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(valor);

        var normalizado = valor.Trim().ToLowerInvariant();
        if (!Formato().IsMatch(normalizado))
        {
            throw new ArgumentException($"Email invalido: '{valor}'.", nameof(valor));
        }

        Valor = normalizado;
    }

    public string Valor { get; }

    public override string ToString() => Valor;

    /// <summary>Tenta criar um Email sem lancar (fluxo de negocio esperado, ex.: validacao de entrada).</summary>
    public static bool TryParse(string? valor, out Email? email)
    {
        email = null;
        if (string.IsNullOrWhiteSpace(valor))
        {
            return false;
        }

        var normalizado = valor.Trim().ToLowerInvariant();
        if (!Formato().IsMatch(normalizado))
        {
            return false;
        }

        email = new Email(normalizado);
        return true;
    }

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 250)]
    private static partial Regex Formato();
}
