using Levante.Conteudo.Domain.Artigos;
using Shouldly;
using Xunit;

namespace Levante.Conteudo.UnitTests.Artigos;

[Trait("Category", "Unit")]
public sealed class TagTests
{
    [Fact]
    public void Criar_normalizaParaMinusculasETrim()
    {
        var tag = new Tag("  Clean-Architecture  ");

        tag.Valor.ShouldBe("clean-architecture");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("com espaco")]
    [InlineData("acento-ção")]
    [InlineData("-comeca-com-hifen")]
    public void Criar_rejeitaInvalida(string valor)
    {
        Should.Throw<ArgumentException>(() => new Tag(valor));
    }

    [Fact]
    public void TryParse_aceitaValida()
    {
        var ok = Tag.TryParse("Dotnet-10", out var tag);

        ok.ShouldBeTrue();
        tag!.Valor.ShouldBe("dotnet-10");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("nao valido")]
    public void TryParse_rejeitaInvalida(string? valor)
    {
        Tag.TryParse(valor, out var tag).ShouldBeFalse();
        tag.ShouldBeNull();
    }

    [Fact]
    public void Igualdade_porValor()
    {
        new Tag("foo").ShouldBe(new Tag("FOO"));
    }
}
