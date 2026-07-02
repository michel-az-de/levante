using FluentValidation;
using Levante.Engajamento.Application.Ports;
using Levante.Engajamento.Domain.Comentarios;
using Levante.SharedKernel;

namespace Levante.Engajamento.Application.Comentarios.CriarComentario;

/// <summary>
/// Cria um comentario pendente de moderacao (handler direto, GAP-F). Anti-spam:
/// honeypot (aceita e descarta em silencio, sem revelar). IP+UA viram hash; o IP
/// cru nunca e persistido.
/// </summary>
public sealed class CriarComentarioCommandHandler(
    IComentarioRepository repositorio,
    IGeradorDeOrigemHash gerador,
    IValidator<CriarComentarioCommand> validador)
{
    public async Task<Result> Handle(CriarComentarioCommand comando, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(comando);

        // Honeypot: bots preenchem o campo escondido. Aceita em silencio e descarta
        // (nao revela a armadilha nem gera ruido na fila de moderacao).
        if (!string.IsNullOrWhiteSpace(comando.Armadilha))
        {
            return Result.Ok();
        }

        var validacao = await validador.ValidateAsync(comando, ct);
        if (!validacao.IsValid)
        {
            return Result.Falha(ErroDeValidacao.De(validacao));
        }

        var origemHash = gerador.Gerar(comando.Ip, comando.UserAgent);
        var comentario = Comentario.Criar(
            comando.ArtigoId,
            comando.ArtigoSlug,
            comando.Autor,
            new TextoComentario(comando.Texto),
            comando.Visitante,
            origemHash);

        await repositorio.AddAsync(comentario, ct);
        return Result.Ok();
    }
}
