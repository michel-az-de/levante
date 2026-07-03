using FluentValidation;

namespace Levante.Audiencia.Application.Assinantes.ConfirmarAssinatura;

/// <summary>Valida a confirmacao: exige o token.</summary>
public sealed class ConfirmarAssinaturaCommandValidator : AbstractValidator<ConfirmarAssinaturaCommand>
{
    public ConfirmarAssinaturaCommandValidator() => RuleFor(x => x.Token).NotEmpty();
}
