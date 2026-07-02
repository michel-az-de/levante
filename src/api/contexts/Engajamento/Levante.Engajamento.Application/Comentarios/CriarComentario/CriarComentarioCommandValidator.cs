using FluentValidation;
using Levante.Engajamento.Domain.Comentarios;

namespace Levante.Engajamento.Application.Comentarios.CriarComentario;

/// <summary>Valida o comentario (o honeypot e tratado no handler, nao aqui).</summary>
public sealed class CriarComentarioCommandValidator : AbstractValidator<CriarComentarioCommand>
{
    public CriarComentarioCommandValidator()
    {
        RuleFor(x => x.ArtigoId).NotEmpty();
        RuleFor(x => x.ArtigoSlug).NotEmpty();
        RuleFor(x => x.Visitante).NotEmpty();
        RuleFor(x => x.Autor).NotEmpty().MaximumLength(Comentario.TamanhoMaximoAutor);
        RuleFor(x => x.Texto)
            .NotEmpty()
            .Must(texto => TextoComentario.TryParse(texto, out _))
            .WithMessage($"Comentario deve ter ate {TextoComentario.TamanhoMaximo} caracteres.");
    }
}
