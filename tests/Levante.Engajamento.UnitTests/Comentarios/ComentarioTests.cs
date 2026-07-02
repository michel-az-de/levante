using Levante.Engajamento.Domain.Comentarios;
using Shouldly;
using Xunit;

namespace Levante.Engajamento.UnitTests.Comentarios;

[Trait("Category", "Unit")]
public sealed class ComentarioTests
{
    private static Comentario Novo() =>
        Comentario.Criar(Guid.NewGuid(), "meu-artigo", "Ana", new TextoComentario("Bom texto."), "v-1", "hash");

    [Fact]
    public void Criar_nasceProntoParaModeracaoComEvento()
    {
        var comentario = Novo();

        comentario.Status.ShouldBe(StatusComentario.Pendente);
        comentario.DataModeracao.ShouldBeNull();
        comentario.DataCriacao.Kind.ShouldBe(DateTimeKind.Utc);
        comentario.Eventos.OfType<ComentarioCriado>().ShouldHaveSingleItem();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Criar_rejeitaAutorVazio(string autor)
    {
        Should.Throw<ArgumentException>(() =>
            Comentario.Criar(Guid.NewGuid(), "meu-artigo", autor, new TextoComentario("Ok."), "v-1", "hash"));
    }

    [Fact]
    public void Criar_rejeitaArtigoVazio()
    {
        Should.Throw<ArgumentException>(() =>
            Comentario.Criar(Guid.Empty, "meu-artigo", "Ana", new TextoComentario("Ok."), "v-1", "hash"));
    }

    [Fact]
    public void Aprovar_mudaStatusERegistraEvento()
    {
        var comentario = Novo();

        comentario.Aprovar();

        comentario.Status.ShouldBe(StatusComentario.Aprovado);
        comentario.DataModeracao.ShouldNotBeNull();
        comentario.Eventos.OfType<ComentarioAprovado>().ShouldHaveSingleItem();
    }

    [Fact]
    public void Rejeitar_mudaStatusSemEventoDeAprovacao()
    {
        var comentario = Novo();

        comentario.Rejeitar();

        comentario.Status.ShouldBe(StatusComentario.Rejeitado);
        comentario.DataModeracao.ShouldNotBeNull();
        comentario.Eventos.OfType<ComentarioAprovado>().ShouldBeEmpty();
    }

    [Fact]
    public void Aprovar_eIdempotente()
    {
        var comentario = Novo();

        comentario.Aprovar();
        comentario.Aprovar();

        comentario.Eventos.OfType<ComentarioAprovado>().ShouldHaveSingleItem();
    }
}
