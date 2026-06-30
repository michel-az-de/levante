using FluentValidation;
using Levante.Conteudo.Domain.Artigos;

namespace Levante.Conteudo.Application.Artigos.EditarArtigo;

/// <summary>Validador do comando de edicao (mesmas regras de campo da criacao + Id).</summary>
public sealed class EditarArtigoCommandValidator : AbstractValidator<EditarArtigoCommand>
{
    public EditarArtigoCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();

        RuleFor(x => x.Titulo).NotEmpty().MaximumLength(200);

        RuleFor(x => x.Slug)
            .NotEmpty()
            .Must(slug => Slug.TryParse(slug, out _))
            .WithMessage("Slug invalido. Use kebab-case (ex.: clean-architecture-na-pratica).");

        RuleFor(x => x.Resumo).NotEmpty().MaximumLength(Artigo.TamanhoMaximoResumo);

        RuleFor(x => x.Conteudo).NotEmpty();
    }
}
