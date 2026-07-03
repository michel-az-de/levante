using Levante.Audiencia.Application.Assinantes.SolicitarAssinatura;
using Levante.Audiencia.Domain.Assinantes;
using Levante.SharedKernel;
using Shouldly;
using Xunit;

namespace Levante.Audiencia.UnitTests.Assinantes;

[Trait("Category", "Unit")]
public sealed class SolicitarAssinaturaCommandHandlerTests
{
    private static SolicitarAssinaturaCommandHandler Criar(AssinanteRepositorioEmMemoria repo) =>
        new(repo, new SolicitarAssinaturaCommandValidator());

    [Fact]
    public async Task Handle_inscreveNovoEmail()
    {
        var repo = new AssinanteRepositorioEmMemoria();

        var resultado = await Criar(repo).Handle(
            new SolicitarAssinaturaCommand("nova@pessoa.com", null), CancellationToken.None);

        resultado.Sucesso.ShouldBeTrue();
        repo.Adicionados.ShouldBe(1);
    }

    [Fact]
    public async Task Handle_honeypotPreenchido_descartaEmSilencio()
    {
        var repo = new AssinanteRepositorioEmMemoria();

        var resultado = await Criar(repo).Handle(
            new SolicitarAssinaturaCommand("bot@pessoa.com", "sou-um-bot"), CancellationToken.None);

        // Aceito (nao revela a armadilha), mas nada e persistido.
        resultado.Sucesso.ShouldBeTrue();
        repo.Adicionados.ShouldBe(0);
    }

    [Theory]
    [InlineData("")]
    [InlineData("invalido")]
    public async Task Handle_emailInvalido_falhaValidacao(string email)
    {
        var repo = new AssinanteRepositorioEmMemoria();

        var resultado = await Criar(repo).Handle(
            new SolicitarAssinaturaCommand(email, null), CancellationToken.None);

        resultado.Falhou.ShouldBeTrue();
        resultado.Erro.Tipo.ShouldBe(TipoErro.Validacao);
        repo.Adicionados.ShouldBe(0);
    }

    [Fact]
    public async Task Handle_emailJaCadastrado_idempotenteSemVazar()
    {
        var repo = new AssinanteRepositorioEmMemoria(Assinante.Solicitar(new Email("ja@existe.com")));

        var resultado = await Criar(repo).Handle(
            new SolicitarAssinaturaCommand("ja@existe.com", null), CancellationToken.None);

        // Mesma resposta de sucesso (nao revela que existe); nada novo persistido.
        resultado.Sucesso.ShouldBeTrue();
        repo.Adicionados.ShouldBe(0);
    }
}
