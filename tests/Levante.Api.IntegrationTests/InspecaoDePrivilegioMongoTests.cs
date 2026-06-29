using Levante.Conteudo.Infrastructure.Seguranca;
using Shouldly;
using Xunit;

namespace Levante.Api.IntegrationTests;

/// <summary>
/// Testes puros do detector (sem Docker): rodam no gate de unidade. Provam a
/// logica de <see cref="InspecaoDePrivilegioMongo.EhPrivilegioAdministrativo"/>
/// nos dois sentidos.
/// </summary>
public sealed class InspecaoDePrivilegioMongoTests
{
    [Theory]
    [InlineData("root")]
    [InlineData("dbOwner")]
    [InlineData("userAdminAnyDatabase")]
    [InlineData("clusterAdmin")]
    [InlineData("readWriteAnyDatabase")]
    [InlineData("atlasAdmin")]
    public void Detecta_papeisAdministrativos(string papel)
    {
        var papeis = new[] { new PapelMongo(papel, "admin") };

        InspecaoDePrivilegioMongo.EhPrivilegioAdministrativo(papeis, []).ShouldBeTrue();
    }

    [Fact]
    public void Aceita_readWriteNaDatabaseDeNegocio()
    {
        var papeis = new[] { new PapelMongo("readWrite", "levante") };
        var privilegios = new[] { new PrivilegioMongo(AnyResource: false, ["find", "insert", "update"]) };

        InspecaoDePrivilegioMongo.EhPrivilegioAdministrativo(papeis, privilegios).ShouldBeFalse();
    }

    [Fact]
    public void Detecta_anyResource()
    {
        var privilegios = new[] { new PrivilegioMongo(AnyResource: true, []) };

        InspecaoDePrivilegioMongo.EhPrivilegioAdministrativo([], privilegios).ShouldBeTrue();
    }

    [Theory]
    [InlineData("createUser")]
    [InlineData("dropDatabase")]
    [InlineData("grantRole")]
    [InlineData("shutdown")]
    public void Detecta_acoesPerigosas(string acao)
    {
        var privilegios = new[] { new PrivilegioMongo(AnyResource: false, [acao]) };

        InspecaoDePrivilegioMongo.EhPrivilegioAdministrativo([], privilegios).ShouldBeTrue();
    }
}
