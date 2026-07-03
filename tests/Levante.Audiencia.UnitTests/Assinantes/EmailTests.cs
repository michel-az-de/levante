using Levante.Audiencia.Domain.Assinantes;
using Shouldly;
using Xunit;

namespace Levante.Audiencia.UnitTests.Assinantes;

[Trait("Category", "Unit")]
public sealed class EmailTests
{
    [Theory]
    [InlineData("a@b.com", "a@b.com")]
    [InlineData("Pessoa@Exemplo.COM", "pessoa@exemplo.com")]
    [InlineData("  x@y.co  ", "x@y.co")]
    public void Ctor_aceitaValido_normalizaMinusculas(string entrada, string esperado)
    {
        var email = new Email(entrada);

        email.Valor.ShouldBe(esperado);
        email.ToString().ShouldBe(esperado);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("sem-arroba")]
    [InlineData("a@b")]
    public void Ctor_rejeitaInvalido(string entrada) =>
        Should.Throw<ArgumentException>(() => new Email(entrada));

    [Fact]
    public void TryParse_valido_retornaTrue()
    {
        Email.TryParse("a@b.com", out var email).ShouldBeTrue();
        email!.Valor.ShouldBe("a@b.com");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("invalido")]
    public void TryParse_invalido_retornaFalse(string? entrada)
    {
        Email.TryParse(entrada, out var email).ShouldBeFalse();
        email.ShouldBeNull();
    }
}
