using FluentValidation;

namespace Levante.Audiencia.Application.Assinantes.CancelarAssinatura;

/// <summary>Valida o cancelamento: exige o token.</summary>
public sealed class CancelarAssinaturaCommandValidator : AbstractValidator<CancelarAssinaturaCommand>
{
    public CancelarAssinaturaCommandValidator() => RuleFor(x => x.Token).NotEmpty();
}
