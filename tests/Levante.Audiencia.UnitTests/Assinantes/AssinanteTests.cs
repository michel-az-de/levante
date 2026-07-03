using Levante.Audiencia.Domain.Assinantes;
using Shouldly;
using Xunit;

namespace Levante.Audiencia.UnitTests.Assinantes;

[Trait("Category", "Unit")]
public sealed class AssinanteTests
{
    private static Assinante Novo() => Assinante.Solicitar(new Email("pessoa@exemplo.com"));

    [Fact]
    public void Solicitar_nascePendenteComTokenEEvento()
    {
        var assinante = Novo();

        assinante.Status.ShouldBe(StatusAssinante.Pendente);
        assinante.DataConfirmacao.ShouldBeNull();
        assinante.DataCancelamento.ShouldBeNull();
        assinante.DataCriacao.Kind.ShouldBe(DateTimeKind.Utc);
        assinante.Token.Valor.ShouldNotBeNullOrWhiteSpace();

        var evento = assinante.Eventos.OfType<AssinaturaSolicitada>().ShouldHaveSingleItem();
        evento.Email.ShouldBe("pessoa@exemplo.com");
        evento.Token.ShouldBe(assinante.Token.Valor);
    }

    [Fact]
    public void Confirmar_mudaParaConfirmadoComEvento()
    {
        var assinante = Novo();
        assinante.LimparEventos();

        assinante.Confirmar();

        assinante.Status.ShouldBe(StatusAssinante.Confirmado);
        assinante.DataConfirmacao.ShouldNotBeNull();
        assinante.Eventos.OfType<AssinanteConfirmado>().ShouldHaveSingleItem();
    }

    [Fact]
    public void Confirmar_eIdempotente()
    {
        var assinante = Novo();
        assinante.Confirmar();
        assinante.LimparEventos();

        assinante.Confirmar();

        assinante.Status.ShouldBe(StatusAssinante.Confirmado);
        assinante.Eventos.ShouldBeEmpty();
    }

    [Fact]
    public void Cancelar_mudaParaCanceladoComEvento()
    {
        var assinante = Novo();
        assinante.LimparEventos();

        assinante.Cancelar();

        assinante.Status.ShouldBe(StatusAssinante.Cancelado);
        assinante.DataCancelamento.ShouldNotBeNull();
        assinante.Eventos.OfType<AssinaturaCancelada>().ShouldHaveSingleItem();
    }

    [Fact]
    public void Cancelar_eIdempotente()
    {
        var assinante = Novo();
        assinante.Cancelar();
        assinante.LimparEventos();

        assinante.Cancelar();

        assinante.Eventos.ShouldBeEmpty();
    }

    [Fact]
    public void Confirmar_aposCancelar_naoRessuscita()
    {
        var assinante = Novo();
        assinante.Cancelar();
        assinante.LimparEventos();

        assinante.Confirmar();

        assinante.Status.ShouldBe(StatusAssinante.Cancelado);
        assinante.Eventos.ShouldBeEmpty();
    }

    [Fact]
    public void Reconstituir_preservaEstadoSemEventos()
    {
        var id = Guid.NewGuid();

        var assinante = Assinante.Reconstituir(
            id,
            new Email("x@y.com"),
            StatusAssinante.Confirmado,
            new TokenConfirmacao(new string('a', 40)),
            DateTime.UtcNow,
            DateTime.UtcNow,
            dataCancelamento: null);

        assinante.Id.ShouldBe(id);
        assinante.Status.ShouldBe(StatusAssinante.Confirmado);
        assinante.Eventos.ShouldBeEmpty();
    }
}
