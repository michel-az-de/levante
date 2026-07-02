using Levante.Identity.Domain.Administradores;
using Shouldly;
using Xunit;

namespace Levante.Identity.UnitTests;

[Trait("Category", "Unit")]
public sealed class EmailTests
{
    [Theory]
    [InlineData("a@b.com", "a@b.com")]
    [InlineData("  Foo.Bar@Example.COM  ", "foo.bar@example.com")]
    public void Construtor_aceitaENormaliza(string entrada, string esperado)
    {
        new Email(entrada).Valor.ShouldBe(esperado);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("sem-arroba")]
    [InlineData("a@b")]
    [InlineData("a @b.com")]
    public void Construtor_rejeitaInvalido(string entrada)
    {
        Should.Throw<ArgumentException>(() => new Email(entrada));
    }

    [Fact]
    public void TryParse_normalizaERetornaTrue()
    {
        Email.TryParse("  Foo@Bar.com ", out var email).ShouldBeTrue();
        email.ShouldNotBeNull();
        email.Valor.ShouldBe("foo@bar.com");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("invalido")]
    public void TryParse_retornaFalseParaInvalido(string? entrada)
    {
        Email.TryParse(entrada, out var email).ShouldBeFalse();
        email.ShouldBeNull();
    }
}
