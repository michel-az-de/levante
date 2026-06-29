using Levante.Api.IntegrationTests.Fixtures;
using Levante.Conteudo.Infrastructure.Seguranca;
using Shouldly;
using Xunit;

namespace Levante.Api.IntegrationTests;

/// <summary>
/// Valida o detector de privilegio minimo contra usuarios REAIS do Mongo.
/// Fecha o caso EasyStok: a conta de runtime nao pode ser administrativa.
/// </summary>
[Trait("Category", "Integration")]
public sealed class MongoPrivilegioMinimoTests(MongoAuthFixture fixture) : IClassFixture<MongoAuthFixture>
{
    [Fact]
    public async Task ContaApp_readWrite_naoEhAdministrativa()
    {
        var status = await InspecaoDePrivilegioMongo.LerStatusDaConexaoAsync(
            fixture.AppClient.GetDatabase("levante"), CancellationToken.None);

        InspecaoDePrivilegioMongo.EhPrivilegioAdministrativo(status.Papeis, status.Privilegios)
            .ShouldBeFalse();
    }

    [Fact]
    public async Task ContaRoot_ehAdministrativa_provaQueODetectorPega()
    {
        var status = await InspecaoDePrivilegioMongo.LerStatusDaConexaoAsync(
            fixture.RootClient.GetDatabase("admin"), CancellationToken.None);

        InspecaoDePrivilegioMongo.EhPrivilegioAdministrativo(status.Papeis, status.Privilegios)
            .ShouldBeTrue();
    }
}
