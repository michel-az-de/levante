using Levante.Conteudo.Domain.Artigos;
using Shouldly;
using Xunit;

namespace Levante.Conteudo.UnitTests.Artigos;

public sealed class ArtigoTests
{
    [Fact]
    public void Criar_iniciaEmRascunhoSemPublicacao()
    {
        var artigo = Artigo.Criar("Titulo", new Slug("titulo"), "Resumo.", "Conteudo do artigo.");

        artigo.Id.ShouldNotBe(Guid.Empty);
        artigo.Status.ShouldBe(StatusArtigo.Rascunho);
        artigo.DataPublicacao.ShouldBeNull();
        artigo.DataCriacao.Kind.ShouldBe(DateTimeKind.Utc);
        artigo.Eventos.ShouldBeEmpty();
    }

    [Fact]
    public void Publicar_mudaStatusRegistraDataEEvento()
    {
        var artigo = Artigo.Criar("Titulo", new Slug("titulo"), "Resumo.", "Conteudo do artigo.");

        artigo.Publicar();

        artigo.Status.ShouldBe(StatusArtigo.Publicado);
        artigo.DataPublicacao.ShouldNotBeNull();
        artigo.Eventos.OfType<ArtigoPublicado>().ShouldHaveSingleItem();
    }

    [Fact]
    public void Publicar_ehIdempotente()
    {
        var artigo = Artigo.Criar("Titulo", new Slug("titulo"), "Resumo.", "Conteudo do artigo.");

        artigo.Publicar();
        artigo.Publicar();

        artigo.Eventos.OfType<ArtigoPublicado>().Count().ShouldBe(1);
    }

    [Fact]
    public void LimparEventos_esvaziaAColecao()
    {
        var artigo = Artigo.Criar("Titulo", new Slug("titulo"), "Resumo.", "Conteudo do artigo.");
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
            id, "Titulo", new Slug("titulo"), "Resumo", "Conteudo", StatusArtigo.Publicado, criacao, publicacao);

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
        Should.Throw<ArgumentException>(() => Artigo.Criar(titulo, new Slug("titulo"), "Resumo.", "Conteudo."));
    }

    [Fact]
    public void Editar_atualizaCamposEditaveis()
    {
        var artigo = Artigo.Criar("Titulo", new Slug("titulo"), "Resumo.", "Conteudo.");

        artigo.Editar("Novo titulo", new Slug("novo-slug"), "Novo resumo.", "Novo conteudo.");

        artigo.Titulo.ShouldBe("Novo titulo");
        artigo.Slug.Valor.ShouldBe("novo-slug");
        artigo.Resumo.ShouldBe("Novo resumo.");
        artigo.Conteudo.ShouldBe("Novo conteudo.");
    }

    [Fact]
    public void Editar_naoAlteraStatusNemPublicacao()
    {
        var artigo = Artigo.Criar("Titulo", new Slug("titulo"), "Resumo.", "Conteudo.");
        artigo.Publicar();
        var publicacao = artigo.DataPublicacao;

        artigo.Editar("Editado", new Slug("titulo"), "Resumo.", "Conteudo.");

        artigo.Status.ShouldBe(StatusArtigo.Publicado);
        artigo.DataPublicacao.ShouldBe(publicacao);
    }

    [Fact]
    public void Editar_rejeitaResumoAlemDoLimite()
    {
        var artigo = Artigo.Criar("Titulo", new Slug("titulo"), "Resumo.", "Conteudo.");
        var resumoLongo = new string('a', Artigo.TamanhoMaximoResumo + 1);

        Should.Throw<ArgumentException>(
            () => artigo.Editar("Titulo", new Slug("titulo"), resumoLongo, "Conteudo."));
    }

    [Fact]
    public void Arquivar_mudaStatusParaArquivado()
    {
        var artigo = Artigo.Criar("Titulo", new Slug("titulo"), "Resumo.", "Conteudo.");
        artigo.Publicar();

        artigo.Arquivar();

        artigo.Status.ShouldBe(StatusArtigo.Arquivado);
    }

    [Fact]
    public void Arquivar_ehIdempotente()
    {
        var artigo = Artigo.Criar("Titulo", new Slug("titulo"), "Resumo.", "Conteudo.");

        artigo.Arquivar();
        artigo.Arquivar();

        artigo.Status.ShouldBe(StatusArtigo.Arquivado);
    }

    [Fact]
    public void Criar_semMeta_usaMetaVazia()
    {
        var artigo = Artigo.Criar("Titulo", new Slug("titulo"), "Resumo.", "Conteudo.");

        artigo.Meta.ShouldBe(MetaSeo.Vazio);
        artigo.Meta.Titulo.ShouldBeNull();
    }

    [Fact]
    public void Criar_comMeta_preservaMeta()
    {
        var meta = MetaSeo.Criar("SEO", "Desc SEO", "/og.png");

        var artigo = Artigo.Criar("Titulo", new Slug("titulo"), "Resumo.", "Conteudo.", meta);

        artigo.Meta.Titulo.ShouldBe("SEO");
        artigo.Meta.Descricao.ShouldBe("Desc SEO");
        artigo.Meta.ImagemOgUrl.ShouldBe("/og.png");
    }

    [Fact]
    public void Editar_atualizaMeta()
    {
        var artigo = Artigo.Criar("Titulo", new Slug("titulo"), "Resumo.", "Conteudo.", MetaSeo.Criar("Antigo", null, null));

        artigo.Editar("Titulo", new Slug("titulo"), "Resumo.", "Conteudo.", MetaSeo.Criar("Novo", "Nova desc", null));

        artigo.Meta.Titulo.ShouldBe("Novo");
        artigo.Meta.Descricao.ShouldBe("Nova desc");
    }

    [Fact]
    public void Criar_semCategoriaETags_usaPadroes()
    {
        var artigo = Artigo.Criar("Titulo", new Slug("titulo"), "Resumo.", "Conteudo.");

        artigo.CategoriaId.ShouldBeNull();
        artigo.Tags.ShouldBeEmpty();
    }

    [Fact]
    public void Criar_comCategoriaETags_preserva()
    {
        var categoriaId = Guid.NewGuid();

        var artigo = Artigo.Criar(
            "Titulo", new Slug("titulo"), "Resumo.", "Conteudo.",
            categoriaId: categoriaId, tags: [new Tag("clean-architecture"), new Tag("ddd")]);

        artigo.CategoriaId.ShouldBe(categoriaId);
        artigo.Tags.Select(t => t.Valor).ShouldBe(["clean-architecture", "ddd"]);
    }

    [Fact]
    public void Editar_atualizaCategoriaETags()
    {
        var artigo = Artigo.Criar(
            "Titulo", new Slug("titulo"), "Resumo.", "Conteudo.",
            categoriaId: Guid.NewGuid(), tags: [new Tag("antiga")]);
        var novaCategoria = Guid.NewGuid();

        artigo.Editar(
            "Titulo", new Slug("titulo"), "Resumo.", "Conteudo.",
            categoriaId: novaCategoria, tags: [new Tag("nova")]);

        artigo.CategoriaId.ShouldBe(novaCategoria);
        artigo.Tags.Select(t => t.Valor).ShouldBe(["nova"]);
    }
}
