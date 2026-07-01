using Levante.Identity.Domain.Administradores;
using Shouldly;
using Xunit;

namespace Levante.Identity.UnitTests;

[Trait("Category", "Unit")]
public sealed class AdministradorTests
{
    private static Administrador Novo() => Administrador.Criar(new Email("admin@levante.dev"), "hash");

    [Fact]
    public void Criar_iniciaAtivoSemBloqueio()
    {
        var admin = Novo();

        admin.Ativo.ShouldBeTrue();
        admin.TentativasDeLoginFalhas.ShouldBe(0);
        admin.BloqueadoAte.ShouldBeNull();
        admin.DataCriacao.Kind.ShouldBe(DateTimeKind.Utc);
    }

    [Fact]
    public void RegistrarFalha_bloqueiaAposOLimite()
    {
        var admin = Novo();

        for (var i = 0; i < Administrador.MaxTentativasDeLoginFalhas; i++)
        {
            admin.RegistrarFalhaDeLogin();
        }

        admin.TentativasDeLoginFalhas.ShouldBe(Administrador.MaxTentativasDeLoginFalhas);
        admin.EstaBloqueado(DateTime.UtcNow).ShouldBeTrue();
    }

    [Fact]
    public void RegistrarFalha_naoBloqueiaAntesDoLimite()
    {
        var admin = Novo();

        admin.RegistrarFalhaDeLogin();

        admin.EstaBloqueado(DateTime.UtcNow).ShouldBeFalse();
    }

    [Fact]
    public void Resetar_limpaFalhasEBloqueio()
    {
        var admin = Novo();
        for (var i = 0; i < Administrador.MaxTentativasDeLoginFalhas + 1; i++)
        {
            admin.RegistrarFalhaDeLogin();
        }

        admin.ResetarFalhas();

        admin.TentativasDeLoginFalhas.ShouldBe(0);
        admin.EstaBloqueado(DateTime.UtcNow).ShouldBeFalse();
    }

    [Fact]
    public void DefinirSenhaHash_atualizaOHash()
    {
        var admin = Novo();

        admin.DefinirSenhaHash("novo-hash");

        admin.SenhaHash.ShouldBe("novo-hash");
    }

    [Fact]
    public void EstaBloqueado_falsoQuandoBloqueioExpirou()
    {
        var admin = Administrador.Reconstituir(
            Guid.NewGuid(), new Email("x@y.com"), "h", 5, DateTime.UtcNow.AddMinutes(-1), true, DateTime.UtcNow);

        admin.EstaBloqueado(DateTime.UtcNow).ShouldBeFalse();
    }
}
