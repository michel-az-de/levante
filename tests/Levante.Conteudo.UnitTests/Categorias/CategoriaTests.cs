using Levante.Conteudo.Domain.Artigos;
using Levante.Conteudo.Domain.Categorias;
using Shouldly;
using Xunit;

namespace Levante.Conteudo.UnitTests.Categorias;

[Trait("Category", "Unit")]
public sealed class CategoriaTests
{
    [Fact]
    public void Criar_preencheCampos()
    {
        var categoria = Categoria.Criar("Arquitetura", new Slug("arquitetura"), "Decisoes e trade-offs.");

        categoria.Id.ShouldNotBe(Guid.Empty);
        categoria.Nome.ShouldBe("Arquitetura");
        categoria.Slug.Valor.ShouldBe("arquitetura");
        categoria.Descricao.ShouldBe("Decisoes e trade-offs.");
        categoria.DataCriacao.Kind.ShouldBe(DateTimeKind.Utc);
    }

    [Fact]
    public void Criar_semDescricao_descricaoNula()
    {
        var categoria = Categoria.Criar("DevOps", new Slug("devops"));

        categoria.Descricao.ShouldBeNull();
    }

    [Fact]
    public void Criar_aparaNomeEDescricao()
    {
        var categoria = Categoria.Criar("  Arquitetura  ", new Slug("arquitetura"), "   ");

        categoria.Nome.ShouldBe("Arquitetura");
        categoria.Descricao.ShouldBeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Criar_rejeitaNomeVazio(string nome)
    {
        Should.Throw<ArgumentException>(() => Categoria.Criar(nome, new Slug("arquitetura")));
    }

    [Fact]
    public void Criar_rejeitaNomeAlemDoLimite()
    {
        var nomeLongo = new string('a', Categoria.TamanhoMaximoNome + 1);

        Should.Throw<ArgumentException>(() => Categoria.Criar(nomeLongo, new Slug("arquitetura")));
    }

    [Fact]
    public void Criar_rejeitaDescricaoAlemDoLimite()
    {
        var descricaoLonga = new string('a', Categoria.TamanhoMaximoDescricao + 1);

        Should.Throw<ArgumentException>(() => Categoria.Criar("Nome", new Slug("nome"), descricaoLonga));
    }

    [Fact]
    public void Editar_atualizaNomeEDescricaoMantemSlug()
    {
        var categoria = Categoria.Criar("Arquitetura", new Slug("arquitetura"), "Antiga.");

        categoria.Editar("Arquitetura de Software", "Nova descricao.");

        categoria.Nome.ShouldBe("Arquitetura de Software");
        categoria.Descricao.ShouldBe("Nova descricao.");
        categoria.Slug.Valor.ShouldBe("arquitetura");
    }

    [Fact]
    public void Editar_rejeitaNomeVazio()
    {
        var categoria = Categoria.Criar("Arquitetura", new Slug("arquitetura"));

        Should.Throw<ArgumentException>(() => categoria.Editar("", null));
    }

    [Fact]
    public void Reconstituir_preservaEstado()
    {
        var id = Guid.NewGuid();
        var criacao = new DateTime(2026, 1, 10, 12, 0, 0, DateTimeKind.Utc);

        var categoria = Categoria.Reconstituir(id, "Arquitetura", new Slug("arquitetura"), "Desc", criacao);

        categoria.Id.ShouldBe(id);
        categoria.DataCriacao.ShouldBe(criacao);
        categoria.Descricao.ShouldBe("Desc");
    }
}
