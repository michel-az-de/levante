namespace Levante.Audiencia.Domain.Assinantes;

/// <summary>Repositorio do agregado <see cref="Assinante"/> (contrato no Domain).</summary>
public interface IAssinanteRepository
{
    /// <summary>Adiciona um assinante. Lanca <see cref="AssinanteJaExisteException"/> se o e-mail ja existir.</summary>
    Task AddAsync(Assinante assinante, CancellationToken ct);

    /// <summary>Busca por token de confirmacao/cancelamento (double opt-in / opt-out).</summary>
    Task<Assinante?> GetByTokenAsync(string token, CancellationToken ct);

    Task UpdateAsync(Assinante assinante, CancellationToken ct);
}
