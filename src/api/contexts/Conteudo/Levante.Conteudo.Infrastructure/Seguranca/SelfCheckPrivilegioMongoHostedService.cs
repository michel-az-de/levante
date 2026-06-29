using Levante.Conteudo.Infrastructure.Persistence;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace Levante.Conteudo.Infrastructure.Seguranca;

/// <summary>
/// Self-check de privilegio minimo no boot. Conecta com a conta de runtime
/// REAL e, se ela tiver privilegio administrativo, FALHA o boot em Producao
/// (fecha o caso EasyStok). Em Dev, apenas registra alerta.
/// </summary>
internal sealed class SelfCheckPrivilegioMongoHostedService(
    ConteudoMongoContext contexto,
    IHostEnvironment ambiente,
    ILogger<SelfCheckPrivilegioMongoHostedService> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            var status = await InspecaoDePrivilegioMongo.LerStatusDaConexaoAsync(
                contexto.Database, cancellationToken);

            if (InspecaoDePrivilegioMongo.EhPrivilegioAdministrativo(status.Papeis, status.Privilegios))
            {
                var papeis = string.Join(", ", status.Papeis.Select(p => $"{p.Role}@{p.Db}"));
                LogConteudo.PrivilegioAdministrativoDetectado(logger, papeis);

                if (ambiente.IsProduction())
                {
                    throw new InvalidOperationException(
                        "Boot abortado: a conta de runtime do MongoDB tem privilegio administrativo, " +
                        "violando o principio de privilegio minimo.");
                }
            }
            else
            {
                LogConteudo.SelfCheckOk(logger);
            }
        }
        catch (MongoException ex)
        {
            // Nao foi possivel inspecionar (ex.: falha de conexao). O readiness
            // (/health/ready) cobre disponibilidade; aqui apenas registramos.
            LogConteudo.SelfCheckFalhou(logger, ex);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
