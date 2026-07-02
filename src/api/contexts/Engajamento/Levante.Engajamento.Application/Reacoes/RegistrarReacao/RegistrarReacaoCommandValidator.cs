using FluentValidation;

namespace Levante.Engajamento.Application.Reacoes.RegistrarReacao;

/// <summary>Valida a reacao: visitante presente, artigo valido e tipo conhecido.</summary>
public sealed class RegistrarReacaoCommandValidator : AbstractValidator<RegistrarReacaoCommand>
{
    public RegistrarReacaoCommandValidator()
    {
        RuleFor(x => x.ArtigoId).NotEmpty();
        RuleFor(x => x.Visitante).NotEmpty();
        RuleFor(x => x.Tipo).IsInEnum().WithMessage("Tipo de reacao invalido.");
    }
}
