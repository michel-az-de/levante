using Levante.Audiencia.Application.Assinantes.CancelarAssinatura;
using Levante.Audiencia.Domain.Assinantes;
using Levante.SharedKernel;
using Shouldly;
using Xunit;

namespace Levante.Audiencia.UnitTests.Assinantes;

[Trait("Category", "Unit")]
public sealed class CancelarAssinaturaCommandHandlerTests
{
    private static CancelarAssinaturaCommandHandler Criar(AssinanteRepositorioEmMemoria repo) =>
        new(repo, new CancelarAssinaturaCommandValidator());

    [Fact]
    public async Task Handle_tokenValido_cancela()
    {
        var assinante = Assinante.Solicitar(new Email("a@b.com"));
        var repo = new AssinanteRepositorioEmMemoria(assinante);

        var resultado = await Criar(repo).Handle(
            new CancelarAssinaturaCommand(assinante.Token.Valor), CancellationToken.None);

        resultado.Sucesso.ShouldBeTrue();
        assinante.Status.ShouldBe(StatusAssinante.Cancelado);
        repo.Atualizados.ShouldBe(1);
    }

    [Fact]
    public async Task Handle_jaCancelado_idempotenteSemAtualizar()
    {
        var assinante = Assinante.Solicitar(new Email("a@b.com"));
        assinante.Cancelar();
        assinante.LimparEventos(); // estado persistido: sem eventos pendentes
        var repo = new AssinanteRepositorioEmMemoria(assinante);

        var resultado = await Criar(repo).Handle(
            new CancelarAssinaturaCommand(assinante.Token.Valor), CancellationToken.None);

        resultado.Sucesso.ShouldBeTrue();
        repo.Atualizados.ShouldBe(0);
    }

    [Fact]
    public async Task Handle_tokenDesconhecido_naoEncontrado()
    {
        var repo = new AssinanteRepositorioEmMemoria();

        var resultado = await Criar(repo).Handle(
            new CancelarAssinaturaCommand(new string('a', 40)), CancellationToken.None);

        resultado.Falhou.ShouldBeTrue();
        resultado.Erro.Tipo.ShouldBe(TipoErro.NaoEncontrado);
    }
}
