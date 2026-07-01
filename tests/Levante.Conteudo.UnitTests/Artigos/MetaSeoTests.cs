using Levante.Conteudo.Domain.Artigos;
using Shouldly;
using Xunit;

namespace Levante.Conteudo.UnitTests.Artigos;

[Trait("Category", "Unit")]
public sealed class MetaSeoTests
{
    [Fact]
    public void Vazio_temTodosOsCamposNulos()
    {
        MetaSeo.Vazio.Titulo.ShouldBeNull();
        MetaSeo.Vazio.Descricao.ShouldBeNull();
        MetaSeo.Vazio.ImagemOgUrl.ShouldBeNull();
    }

    [Fact]
    public void Criar_preservaOsValores()
    {
        var meta = MetaSeo.Criar("Titulo SEO", "Descricao SEO", "/og/imagem.png");

        meta.Titulo.ShouldBe("Titulo SEO");
        meta.Descricao.ShouldBe("Descricao SEO");
        meta.ImagemOgUrl.ShouldBe("/og/imagem.png");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Criar_normalizaVazioParaNull(string? valor)
    {
        var meta = MetaSeo.Criar(valor, valor, valor);

        meta.Titulo.ShouldBeNull();
        meta.Descricao.ShouldBeNull();
        meta.ImagemOgUrl.ShouldBeNull();
    }

    [Fact]
    public void Criar_aparaEspacos()
    {
        var meta = MetaSeo.Criar("  Titulo  ", "  Descricao  ", "  /img  ");

        meta.Titulo.ShouldBe("Titulo");
        meta.Descricao.ShouldBe("Descricao");
        meta.ImagemOgUrl.ShouldBe("/img");
    }

    [Fact]
    public void Criar_rejeitaTituloAlemDoLimite()
    {
        var tituloLongo = new string('a', MetaSeo.TamanhoMaximoTitulo + 1);

        Should.Throw<ArgumentException>(() => MetaSeo.Criar(tituloLongo, null, null));
    }

    [Fact]
    public void Criar_rejeitaDescricaoAlemDoLimite()
    {
        var descricaoLonga = new string('a', MetaSeo.TamanhoMaximoDescricao + 1);

        Should.Throw<ArgumentException>(() => MetaSeo.Criar(null, descricaoLonga, null));
    }

    [Fact]
    public void Criar_aceitaNoLimite()
    {
        var meta = MetaSeo.Criar(
            new string('a', MetaSeo.TamanhoMaximoTitulo),
            new string('b', MetaSeo.TamanhoMaximoDescricao),
            null);

        meta.Titulo!.Length.ShouldBe(MetaSeo.TamanhoMaximoTitulo);
        meta.Descricao!.Length.ShouldBe(MetaSeo.TamanhoMaximoDescricao);
    }
}
