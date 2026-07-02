using Levante.Engajamento.Application.Comentarios.AprovarComentario;
using Levante.Engajamento.Application.Comentarios.ListarComentariosAprovados;
using Levante.Engajamento.Application.Comentarios.ListarComentariosPendentes;
using Levante.Engajamento.Application.Comentarios.RejeitarComentario;
using Levante.Engajamento.Domain.Comentarios;
using Levante.SharedKernel;
using Shouldly;
using Xunit;

namespace Levante.Engajamento.UnitTests.Comentarios;

[Trait("Category", "Unit")]
public sealed class ComentarioModeracaoTests
{
    private static Comentario Pendente(Guid artigoId, string autor = "Ana") =>
        Comentario.Criar(artigoId, "meu-artigo", autor, new TextoComentario("Texto."), "v-1", "hash");

    [Fact]
    public async Task Aprovar_marcaComoAprovado()
    {
        var comentario = Pendente(Guid.NewGuid());
        var repo = new ComentarioRepositorioEmMemoria(comentario);
        var handler = new AprovarComentarioCommandHandler(repo);

        var resultado = await handler.Handle(new AprovarComentarioCommand(comentario.Id), CancellationToken.None);

        resultado.Sucesso.ShouldBeTrue();
        comentario.Status.ShouldBe(StatusComentario.Aprovado);
        repo.Atualizados.ShouldBe(1);
    }

    [Fact]
    public async Task Aprovar_naoEncontrado_retornaNaoEncontrado()
    {
        var repo = new ComentarioRepositorioEmMemoria();
        var handler = new AprovarComentarioCommandHandler(repo);

        var resultado = await handler.Handle(new AprovarComentarioCommand(Guid.NewGuid()), CancellationToken.None);

        resultado.Falhou.ShouldBeTrue();
        resultado.Erro.Tipo.ShouldBe(TipoErro.NaoEncontrado);
    }

    [Fact]
    public async Task Rejeitar_marcaComoRejeitado()
    {
        var comentario = Pendente(Guid.NewGuid());
        var repo = new ComentarioRepositorioEmMemoria(comentario);
        var handler = new RejeitarComentarioCommandHandler(repo);

        var resultado = await handler.Handle(new RejeitarComentarioCommand(comentario.Id), CancellationToken.None);

        resultado.Sucesso.ShouldBeTrue();
        comentario.Status.ShouldBe(StatusComentario.Rejeitado);
    }

    [Fact]
    public async Task ListarAprovados_soTrazAprovadosDoArtigo()
    {
        var artigoId = Guid.NewGuid();
        var aprovado = Pendente(artigoId, "Aprovado");
        aprovado.Aprovar();
        var repo = new ComentarioRepositorioEmMemoria(aprovado, Pendente(artigoId, "Pendente"));
        var handler = new ListarComentariosAprovadosQueryHandler(repo);

        var resultado = await handler.Handle(new ListarComentariosAprovadosQuery(artigoId), CancellationToken.None);

        resultado.Valor!.ShouldHaveSingleItem().Autor.ShouldBe("Aprovado");
    }

    [Fact]
    public async Task ListarPendentes_soTrazPendentes()
    {
        var aprovado = Pendente(Guid.NewGuid(), "Aprovado");
        aprovado.Aprovar();
        var repo = new ComentarioRepositorioEmMemoria(aprovado, Pendente(Guid.NewGuid(), "Pendente"));
        var handler = new ListarComentariosPendentesQueryHandler(repo);

        var resultado = await handler.Handle(new ListarComentariosPendentesQuery(), CancellationToken.None);

        resultado.Valor!.ShouldHaveSingleItem().Autor.ShouldBe("Pendente");
    }
}
