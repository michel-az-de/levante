using FluentValidation;
using Levante.Audiencia.Domain.Assinantes;

namespace Levante.Audiencia.Application.Assinantes.SolicitarAssinatura;

/// <summary>Valida a inscricao (o honeypot e tratado no handler, nao aqui).</summary>
public sealed class SolicitarAssinaturaCommandValidator : AbstractValidator<SolicitarAssinaturaCommand>
{
    public SolicitarAssinaturaCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .Must(email => Email.TryParse(email, out _))
            .WithMessage("Email invalido.");
    }
}
