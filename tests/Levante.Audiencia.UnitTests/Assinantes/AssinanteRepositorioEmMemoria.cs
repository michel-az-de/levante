using Levante.Audiencia.Domain.Assinantes;

namespace Levante.Audiencia.UnitTests.Assinantes;

/// <summary>Fake em memoria de <see cref="IAssinanteRepository"/> com contadores.</summary>
internal sealed class AssinanteRepositorioEmMemoria : IAssinanteRepository
{
    private readonly List<Assinante> _assinantes;

    public AssinanteRepositorioEmMemoria(params Assinante[] assinantes) => _assinantes = [.. assinantes];

    public int Adicionados { get; private set; }

    public int Atualizados { get; private set; }

    public Task AddAsync(Assinante assinante, CancellationToken ct)
    {
        if (_assinantes.Any(a => a.Email.Valor == assinante.Email.Valor))
        {
            // Espelha o indice unico do Mongo (a Application trata como idempotente).
            throw new AssinanteJaExisteException(assinante.Email.Valor);
        }

        _assinantes.Add(assinante);
        Adicionados++;
        return Task.CompletedTask;
    }

    public Task<Assinante?> GetByTokenAsync(string token, CancellationToken ct) =>
        Task.FromResult(_assinantes.FirstOrDefault(a => a.Token.Valor == token));

    public Task UpdateAsync(Assinante assinante, CancellationToken ct)
    {
        Atualizados++;
        return Task.CompletedTask;
    }
}
