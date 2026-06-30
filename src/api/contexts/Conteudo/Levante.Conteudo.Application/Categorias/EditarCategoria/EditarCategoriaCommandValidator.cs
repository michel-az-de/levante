using FluentValidation;
using Levante.Conteudo.Domain.Categorias;

namespace Levante.Conteudo.Application.Categorias.EditarCategoria;

/// <summary>Validador da edicao de categoria (slug nao e editavel).</summary>
public sealed class EditarCategoriaCommandValidator : AbstractValidator<EditarCategoriaCommand>
{
    public EditarCategoriaCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Nome).NotEmpty().MaximumLength(Categoria.TamanhoMaximoNome);
        RuleFor(x => x.Descricao).MaximumLength(Categoria.TamanhoMaximoDescricao);
    }
}
