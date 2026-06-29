using Levante.Identity.Application.Ports;
using Levante.Identity.Domain.Administradores;
using Levante.Identity.Infrastructure.Seguranca;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Levante.Identity.Infrastructure.Persistence;

/// <summary>
/// Boot do contexto Identity: garante indices (sempre) e semeia o admin
/// (somente fora de Producao, a partir de user-secrets; nunca senha no repo).
/// </summary>
internal sealed class IdentityInicializacaoHostedService(
    IServiceProvider provider,
    IHostEnvironment ambiente,
    IOptions<AdminSeedOptions> seedOptions,
    ILogger<IdentityInicializacaoHostedService> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var escopo = provider.CreateScope();
        var sp = escopo.ServiceProvider;

        var contexto = sp.GetRequiredService<IdentityMongoContext>();
        await contexto.EnsureIndexesAsync(cancellationToken);

        if (ambiente.IsProduction())
        {
            return; // admin de producao = passo unico seguro, fora do boot
        }

        var seed = seedOptions.Value;
        if (string.IsNullOrWhiteSpace(seed.Email) || string.IsNullOrWhiteSpace(seed.SenhaInicial))
        {
            return; // sem seed configurado
        }

        var repositorio = sp.GetRequiredService<IAdministradorRepository>();
        if (await repositorio.ExisteAlgumAsync(cancellationToken))
        {
            return;
        }

        if (!Email.TryParse(seed.Email, out var email) || email is null)
        {
            return;
        }

        var hash = sp.GetRequiredService<IHashDeSenha>().Hash(seed.SenhaInicial);
        await repositorio.AddAsync(Administrador.Criar(email, hash), cancellationToken);
        LogIdentity.AdminSemeado(logger, email.Valor);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
