using FluentValidation;
using Levante.Conteudo.Domain.Artigos;
using Levante.Conteudo.Domain.Categorias;

namespace Levante.Conteudo.Application.Categorias.CriarCategoria;

/// <summary>Validador da criacao de categoria (formato do slug delegado ao VO do dominio).</summary>
public sealed class CriarCategoriaCommandValidator : AbstractValidator<CriarCategoriaCommand>
{
    public CriarCategoriaCommandValidator()
    {
        RuleFor(x => x.Nome).NotEmpty().MaximumLength(Categoria.TamanhoMaximoNome);

        RuleFor(x => x.Slug)
            .NotEmpty()
            .Must(slug => Slug.TryParse(slug, out _))
            .WithMessage("Slug invalido. Use kebab-case (ex.: arquitetura-de-software).");

        RuleFor(x => x.Descricao).MaximumLength(Categoria.TamanhoMaximoDescricao);
    }
}
