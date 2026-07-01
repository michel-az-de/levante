using FluentValidation;
using Levante.Conteudo.Application.Artigos;
using Levante.Conteudo.Domain.Categorias;
using Levante.SharedKernel;

namespace Levante.Conteudo.Application.Categorias.EditarCategoria;

/// <summary>Edita nome/descricao de uma categoria (handler direto, GAP-F). Slug imutavel.</summary>
public sealed class EditarCategoriaCommandHandler(
    ICategoriaRepository repositorio,
    IValidator<EditarCategoriaCommand> validador)
{
    public async Task<Result<CategoriaResponse>> Handle(EditarCategoriaCommand comando, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(comando);

        var validacao = await validador.ValidateAsync(comando, ct);
        if (!validacao.IsValid)
        {
            return Result.Falha<CategoriaResponse>(ErroDeValidacao.De(validacao));
        }

        var categoria = await repositorio.GetByIdAsync(comando.Id, ct);
        if (categoria is null)
        {
            return Result.Falha<CategoriaResponse>(
                Error.NaoEncontrado("categoria_nao_encontrada", $"Categoria '{comando.Id}' nao encontrada."));
        }

        categoria.Editar(comando.Nome, comando.Descricao);
        await repositorio.UpdateAsync(categoria, ct);

        return Result.Ok(CategoriaResponse.De(categoria));
    }
}
