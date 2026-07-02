namespace Levante.Engajamento.Domain.Comentarios;

/// <summary>Value Object do corpo do comentario. Normaliza (trim) e limita o tamanho.</summary>
public sealed record TextoComentario
{
    public const int TamanhoMaximo = 2000;

    public TextoComentario(string valor)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(valor);

        var normalizado = valor.Trim();
        if (normalizado.Length > TamanhoMaximo)
        {
            throw new ArgumentException($"Comentario excede {TamanhoMaximo} caracteres.", nameof(valor));
        }

        Valor = normalizado;
    }

    public string Valor { get; }

    public override string ToString() => Valor;

    public static bool TryParse(string? valor, out TextoComentario? texto)
    {
        texto = null;
        if (string.IsNullOrWhiteSpace(valor) || valor.Trim().Length > TamanhoMaximo)
        {
            return false;
        }

        texto = new TextoComentario(valor);
        return true;
    }
}
