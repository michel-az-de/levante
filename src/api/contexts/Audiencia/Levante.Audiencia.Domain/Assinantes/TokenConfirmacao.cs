using System.Security.Cryptography;

namespace Levante.Audiencia.Domain.Assinantes;

/// <summary>
/// Value Object do token opaco de confirmacao/cancelamento (double opt-in). Gerado
/// com CSPRNG (256 bits em hex minusculo, URL-safe). Nao identifica pessoa: e um
/// segredo por assinante que autoriza confirmar ou cancelar sem login.
/// </summary>
public sealed record TokenConfirmacao
{
    public const int TamanhoMinimo = 32;

    public TokenConfirmacao(string valor)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(valor);

        var normalizado = valor.Trim();
        if (normalizado.Length < TamanhoMinimo)
        {
            throw new ArgumentException($"Token deve ter ao menos {TamanhoMinimo} caracteres.", nameof(valor));
        }

        Valor = normalizado;
    }

    public string Valor { get; }

    public override string ToString() => Valor;

    /// <summary>Gera um token novo (256 bits, hex minusculo).</summary>
    public static TokenConfirmacao Gerar() =>
        new(Convert.ToHexString(RandomNumberGenerator.GetBytes(32)).ToLowerInvariant());
}
