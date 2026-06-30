using FluentValidation;
using Levante.Conteudo.Domain.Artigos;
using Levante.Conteudo.Domain.Categorias;

namespace Levante.Conteudo.Application.Artigos.CriarArtigo;

/// <summary>Validador do comando de criacao (formato do slug delegado ao VO do dominio).</summary>
public sealed class CriarArtigoCommandValidator : AbstractValidator<CriarArtigoCommand>
{
    public CriarArtigoCommandValidator(ICategoriaRepository categorias)
    {
        RuleFor(x => x.Titulo).NotEmpty().MaximumLength(200);

        RuleFor(x => x.Slug)
            .NotEmpty()
            .Must(slug => Slug.TryParse(slug, out _))
            .WithMessage("Slug invalido. Use kebab-case (ex.: clean-architecture-na-pratica).");

        RuleFor(x => x.Resumo).NotEmpty().MaximumLength(Artigo.TamanhoMaximoResumo);

        RuleFor(x => x.Conteudo).NotEmpty();

        RuleFor(x => x.MetaTitulo).MaximumLength(MetaSeo.TamanhoMaximoTitulo);
        RuleFor(x => x.MetaDescricao).MaximumLength(MetaSeo.TamanhoMaximoDescricao);
        RuleFor(x => x.ImagemOgUrl)
            .Must(SeoUrl.EhImagemOgValida)
            .WithMessage("Imagem OG deve ser uma URL http(s) ou um caminho comecando com '/'.");

        RuleFor(x => x.CategoriaId)
            .MustAsync(async (id, ct) => id is null || await categorias.GetByIdAsync(id.Value, ct) is not null)
            .WithMessage("Categoria informada nao existe.");

        RuleFor(x => x.Tags)
            .Must(tags => tags is null || tags.Count <= AssociacaoArtigo.MaximoDeTags)
            .WithMessage($"Maximo de {AssociacaoArtigo.MaximoDeTags} tags.");
        RuleForEach(x => x.Tags)
            .Must(tag => Tag.TryParse(tag, out _))
            .WithMessage("Tag invalida. Use kebab-case minusculo (ex.: clean-architecture).");
    }
}
