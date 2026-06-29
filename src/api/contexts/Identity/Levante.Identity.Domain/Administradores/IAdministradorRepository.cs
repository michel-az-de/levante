namespace Levante.Identity.Domain.Administradores;

/// <summary>Porta de persistencia do agregado Administrador.</summary>
public interface IAdministradorRepository
{
    Task<Administrador?> GetByEmailAsync(string email, CancellationToken ct);

    Task AddAsync(Administrador administrador, CancellationToken ct);

    Task UpdateAsync(Administrador administrador, CancellationToken ct);

    Task<bool> ExisteAlgumAsync(CancellationToken ct);
}
