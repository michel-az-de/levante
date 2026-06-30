using Levante.Conteudo.Application.Artigos.CriarArtigo;
using Levante.Conteudo.Application.Artigos.EditarArtigo;
using Levante.Conteudo.Domain.Artigos;
using Shouldly;
using Xunit;

namespace Levante.Conteudo.UnitTests.Artigos;

public sealed class ArtigoCommandValidatorsTests
{
    private static readonly string ResumoLongo = new('a', Artigo.TamanhoMaximoResumo + 1);

    [Fact]
    public void Criar_valido()
    {
        var validador = new CriarArtigoCommandValidator();

        var resultado = validador.Validate(
            new CriarArtigoCommand("Titulo", "meu-artigo", "Resumo.", "Conteudo."));

        resultado.IsValid.ShouldBeTrue();
    }

    [Theory]
    [InlineData("", "meu-artigo", "Resumo.", "Conteudo.")] // titulo vazio
    [InlineData("Titulo", "Slug Invalido", "Resumo.", "Conteudo.")] // slug com espaco/maiuscula
    [InlineData("Titulo", "meu-artigo", "", "Conteudo.")] // resumo vazio
    [InlineData("Titulo", "meu-artigo", "Resumo.", "")] // conteudo vazio
    public void Criar_invalido(string titulo, string slug, string resumo, string conteudo)
    {
        var validador = new CriarArtigoCommandValidator();

        var resultado = validador.Validate(new CriarArtigoCommand(titulo, slug, resumo, conteudo));

        resultado.IsValid.ShouldBeFalse();
    }

    [Fact]
    public void Criar_rejeitaResumoAlemDoLimite()
    {
        var validador = new CriarArtigoCommandValidator();

        var resultado = validador.Validate(
            new CriarArtigoCommand("Titulo", "meu-artigo", ResumoLongo, "Conteudo."));

        resultado.IsValid.ShouldBeFalse();
    }

    [Fact]
    public void Criar_rejeitaMetaTituloAlemDe60()
    {
        var validador = new CriarArtigoCommandValidator();
        var metaTituloLongo = new string('a', 61);

        var resultado = validador.Validate(
            new CriarArtigoCommand("Titulo", "meu-artigo", "Resumo.", "Conteudo.", MetaTitulo: metaTituloLongo));

        resultado.IsValid.ShouldBeFalse();
    }

    [Fact]
    public void Criar_rejeitaMetaDescricaoAlemDe155()
    {
        var validador = new CriarArtigoCommandValidator();
        var metaDescricaoLonga = new string('a', 156);

        var resultado = validador.Validate(
            new CriarArtigoCommand("Titulo", "meu-artigo", "Resumo.", "Conteudo.", MetaDescricao: metaDescricaoLonga));

        resultado.IsValid.ShouldBeFalse();
    }

    [Theory]
    [InlineData("javascript:alert(1)")]
    [InlineData("ftp://exemplo.com/i.png")]
    [InlineData("og.png")] // relativa sem barra inicial
    public void Criar_rejeitaImagemOgInvalida(string imagem)
    {
        var validador = new CriarArtigoCommandValidator();

        var resultado = validador.Validate(
            new CriarArtigoCommand("Titulo", "meu-artigo", "Resumo.", "Conteudo.", ImagemOgUrl: imagem));

        resultado.IsValid.ShouldBeFalse();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("/og/imagem.png")]
    [InlineData("https://cdn.exemplo.com/og.png")]
    public void Criar_aceitaImagemOgValida(string? imagem)
    {
        var validador = new CriarArtigoCommandValidator();

        var resultado = validador.Validate(
            new CriarArtigoCommand("Titulo", "meu-artigo", "Resumo.", "Conteudo.", ImagemOgUrl: imagem));

        resultado.IsValid.ShouldBeTrue();
    }

    [Fact]
    public void Editar_rejeitaIdVazio()
    {
        var validador = new EditarArtigoCommandValidator();

        var resultado = validador.Validate(
            new EditarArtigoCommand(Guid.Empty, "Titulo", "meu-artigo", "Resumo.", "Conteudo."));

        resultado.IsValid.ShouldBeFalse();
    }

    [Fact]
    public void Editar_valido()
    {
        var validador = new EditarArtigoCommandValidator();

        var resultado = validador.Validate(
            new EditarArtigoCommand(Guid.NewGuid(), "Titulo", "meu-artigo", "Resumo.", "Conteudo."));

        resultado.IsValid.ShouldBeTrue();
    }
}
