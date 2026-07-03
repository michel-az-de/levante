namespace Levante.Audiencia.Domain.Assinantes;

/// <summary>Ciclo de vida de um <see cref="Assinante"/> da newsletter (double opt-in).</summary>
public enum StatusAssinante
{
    Pendente = 0,
    Confirmado = 1,
    Cancelado = 2,
}
