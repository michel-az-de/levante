using FluentValidation;
using Levante.Conteudo.Domain.Artigos;

namespace Levante.Conteudo.Application.Artigos.CriarArtigo;

/// <summary>Validador do comando de criacao (formato do slug delegado ao VO do dominio).</summary>
public sealed class CriarArtigoCommandValidator : AbstractValidator<CriarArtigoCommand>
{
    public CriarArtigoCommandValidator()
    {
        RuleFor(x => x.Titulo).NotEmpty().MaximumLength(200);

        RuleFor(x => x.Slug)
            .NotEmpty()
            .Must(slug => Slug.TryParse(slug, out _))
            .WithMessage("Slug invalido. Use kebab-case (ex.: clean-architecture-na-pratica).");

        RuleFor(x => x.Resumo).NotEmpty().MaximumLength(Artigo.TamanhoMaximoResumo);

        RuleFor(x => x.Conteudo).NotEmpty();
    }
}
