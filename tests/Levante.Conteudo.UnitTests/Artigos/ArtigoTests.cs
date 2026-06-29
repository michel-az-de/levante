using Levante.Conteudo.Domain.Artigos;
using Shouldly;
using Xunit;

namespace Levante.Conteudo.UnitTests.Artigos;

public sealed class ArtigoTests
{
    [Fact]
    public void Criar_iniciaEmRascunhoSemPublicacao()
    {
        var artigo = Artigo.Criar("Titulo", new Slug("titulo"), "Conteudo do artigo.");

        artigo.Id.ShouldNotBe(Guid.Empty);
        artigo.Status.ShouldBe(StatusArtigo.Rascunho);
        artigo.DataPublicacao.ShouldBeNull();
        artigo.DataCriacao.Kind.ShouldBe(DateTimeKind.Utc);
        artigo.Eventos.ShouldBeEmpty();
    }

    [Fact]
    public void Publicar_mudaStatusRegistraDataEEvento()
    {
        var artigo = Artigo.Criar("Titulo", new Slug("titulo"), "Conteudo do artigo.");

        artigo.Publicar();

        artigo.Status.ShouldBe(StatusArtigo.Publicado);
        artigo.DataPublicacao.ShouldNotBeNull();
        artigo.Eventos.OfType<ArtigoPublicado>().ShouldHaveSingleItem();
    }

    [Fact]
    public void Publicar_ehIdempotente()
    {
        var artigo = Artigo.Criar("Titulo", new Slug("titulo"), "Conteudo do artigo.");

        artigo.Publicar();
        artigo.Publicar();

        artigo.Eventos.OfType<ArtigoPublicado>().Count().ShouldBe(1);
    }

    [Fact]
    public void LimparEventos_esvaziaAColecao()
    {
        var artigo = Artigo.Criar("Titulo", new Slug("titulo"), "Conteudo do artigo.");
        artigo.Publicar();

        artigo.LimparEventos();

        artigo.Eventos.ShouldBeEmpty();
    }

    [Fact]
    public void Reconstituir_preservaOEstadoInformado()
    {
        var id = Guid.NewGuid();
        var criacao = new DateTime(2026, 1, 10, 12, 0, 0, DateTimeKind.Utc);
        var publicacao = new DateTime(2026, 1, 12, 9, 30, 0, DateTimeKind.Utc);

        var artigo = Artigo.Reconstituir(
            id, "Titulo", new Slug("titulo"), "Conteudo", StatusArtigo.Publicado, criacao, publicacao);

        artigo.Id.ShouldBe(id);
        artigo.Status.ShouldBe(StatusArtigo.Publicado);
        artigo.DataCriacao.ShouldBe(criacao);
        artigo.DataPublicacao.ShouldBe(publicacao);
        artigo.Eventos.ShouldBeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Criar_rejeitaTituloVazio(string titulo)
    {
        Should.Throw<ArgumentException>(() => Artigo.Criar(titulo, new Slug("titulo"), "Conteudo."));
    }
}
