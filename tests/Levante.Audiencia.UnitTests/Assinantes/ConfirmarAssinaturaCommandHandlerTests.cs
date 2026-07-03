using Levante.Audiencia.Application.Assinantes.ConfirmarAssinatura;
using Levante.Audiencia.Domain.Assinantes;
using Levante.SharedKernel;
using Shouldly;
using Xunit;

namespace Levante.Audiencia.UnitTests.Assinantes;

[Trait("Category", "Unit")]
public sealed class ConfirmarAssinaturaCommandHandlerTests
{
    private static ConfirmarAssinaturaCommandHandler Criar(AssinanteRepositorioEmMemoria repo) =>
        new(repo, new ConfirmarAssinaturaCommandValidator());

    [Fact]
    public async Task Handle_tokenValido_confirmaEAtualiza()
    {
        var assinante = Assinante.Solicitar(new Email("a@b.com"));
        var repo = new AssinanteRepositorioEmMemoria(assinante);

        var resultado = await Criar(repo).Handle(
            new ConfirmarAssinaturaCommand(assinante.Token.Valor), CancellationToken.None);

        resultado.Sucesso.ShouldBeTrue();
        assinante.Status.ShouldBe(StatusAssinante.Confirmado);
        repo.Atualizados.ShouldBe(1);
    }

    [Fact]
    public async Task Handle_jaConfirmado_idempotenteSemAtualizar()
    {
        var assinante = Assinante.Solicitar(new Email("a@b.com"));
        assinante.Confirmar();
        assinante.LimparEventos(); // estado persistido: sem eventos pendentes
        var repo = new AssinanteRepositorioEmMemoria(assinante);

        var resultado = await Criar(repo).Handle(
            new ConfirmarAssinaturaCommand(assinante.Token.Valor), CancellationToken.None);

        // Idempotente: sucesso sem gravacao (nada mudou).
        resultado.Sucesso.ShouldBeTrue();
        repo.Atualizados.ShouldBe(0);
    }

    [Fact]
    public async Task Handle_tokenDesconhecido_naoEncontrado()
    {
        var repo = new AssinanteRepositorioEmMemoria();

        var resultado = await Criar(repo).Handle(
            new ConfirmarAssinaturaCommand(new string('a', 40)), CancellationToken.None);

        resultado.Falhou.ShouldBeTrue();
        resultado.Erro.Tipo.ShouldBe(TipoErro.NaoEncontrado);
    }

    [Fact]
    public async Task Handle_tokenVazio_falhaValidacao()
    {
        var repo = new AssinanteRepositorioEmMemoria();

        var resultado = await Criar(repo).Handle(
            new ConfirmarAssinaturaCommand(""), CancellationToken.None);

        resultado.Falhou.ShouldBeTrue();
        resultado.Erro.Tipo.ShouldBe(TipoErro.Validacao);
    }
}
