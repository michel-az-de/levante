using Levante.SharedKernel.Infrastructure;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Levante.Conteudo.Infrastructure.HealthChecks;

/// <summary>
/// Readiness do banco: roda <c>ping</c> no Mongo. Registrado com a tag "ready"
/// (o endpoint /health/ready filtra por ela).
/// </summary>
internal sealed class MongoHealthCheck(IMongoClient client, IOptions<MongoOptions> options)
    : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var database = client.GetDatabase(options.Value.DatabaseName);
            await database.RunCommandAsync<BsonDocument>(
                new BsonDocument("ping", 1),
                cancellationToken: cancellationToken);

            return HealthCheckResult.Healthy("MongoDB respondeu ao ping.");
        }
        catch (MongoException ex)
        {
            return HealthCheckResult.Unhealthy("MongoDB nao respondeu ao ping.", ex);
        }
        catch (TimeoutException ex)
        {
            return HealthCheckResult.Unhealthy("MongoDB excedeu o tempo de resposta.", ex);
        }
    }
}
