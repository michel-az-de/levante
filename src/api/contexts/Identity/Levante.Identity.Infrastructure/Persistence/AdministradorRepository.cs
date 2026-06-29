using Levante.Identity.Domain.Administradores;
using MongoDB.Driver;

namespace Levante.Identity.Infrastructure.Persistence;

/// <summary>Repositorio do agregado Administrador (encapsula o MongoDB.Driver).</summary>
internal sealed class AdministradorRepository(IdentityMongoContext contexto) : IAdministradorRepository
{
    public async Task<Administrador?> GetByEmailAsync(string email, CancellationToken ct)
    {
        var doc = await contexto.Administradores
            .Find(d => d.Email == email)
            .FirstOrDefaultAsync(ct);

        return doc?.ParaDominio();
    }

    public Task AddAsync(Administrador administrador, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(administrador);
        return contexto.Administradores.InsertOneAsync(
            AdministradorDocument.DeDominio(administrador), options: null, ct);
    }

    public Task UpdateAsync(Administrador administrador, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(administrador);
        return contexto.Administradores.ReplaceOneAsync(
            d => d.Id == administrador.Id,
            AdministradorDocument.DeDominio(administrador),
            new ReplaceOptions(),
            ct);
    }

    public async Task<bool> ExisteAlgumAsync(CancellationToken ct)
    {
        var total = await contexto.Administradores.CountDocumentsAsync(
            Builders<AdministradorDocument>.Filter.Empty, options: null, ct);
        return total > 0;
    }
}
