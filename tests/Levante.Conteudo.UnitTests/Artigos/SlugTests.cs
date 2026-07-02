using Levante.Conteudo.Domain.Artigos;
using Shouldly;
using Xunit;

namespace Levante.Conteudo.UnitTests.Artigos;

[Trait("Category", "Unit")]
public sealed class SlugTests
{
    [Theory]
    [InlineData("clean-architecture-na-pratica")]
    [InlineData("artigo")]
    [InlineData("a1-b2-c3")]
    public void Construtor_aceitaKebabCaseValido(string valor)
    {
        var slug = new Slug(valor);

        slug.Valor.ShouldBe(valor);
        slug.ToString().ShouldBe(valor);
    }

    [Theory]
    [InlineData("Com-Maiuscula")]
    [InlineData("com espaco")]
    [InlineData("-comeca-com-hifen")]
    [InlineData("termina-com-hifen-")]
    [InlineData("acentuacao-publicacao-invalida-ç")]
    [InlineData("")]
    [InlineData("   ")]
    public void Construtor_rejeitaFormatoInvalido(string valor)
    {
        Should.Throw<ArgumentException>(() => new Slug(valor));
    }
}
