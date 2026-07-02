using FluentValidation;
using Levante.Conteudo.Application.Artigos;
using Levante.Conteudo.Domain.Artigos;
using Levante.Conteudo.Domain.Categorias;
using Levante.SharedKernel;

namespace Levante.Conteudo.Application.Categorias.CriarCategoria;

/// <summary>
/// Cria uma categoria (handler direto, GAP-F). Valida via FluentValidation e pre-checa
/// a unicidade do slug por Result; o indice unico e a rede de seguranca da corrida.
/// </summary>
public sealed class CriarCategoriaCommandHandler(
    ICategoriaRepository repositorio,
    IValidator<CriarCategoriaCommand> validador)
{
    public async Task<Result<CategoriaResponse>> Handle(CriarCategoriaCommand comando, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(comando);

        var validacao = await validador.ValidateAsync(comando, ct);
        if (!validacao.IsValid)
        {
            return Result.Falha<CategoriaResponse>(ErroDeValidacao.De(validacao));
        }

        var existente = await repositorio.GetBySlugAsync(comando.Slug, ct);
        if (existente is not null)
        {
            return SlugEmUso(comando.Slug);
        }

        var categoria = Categoria.Criar(comando.Nome, new Slug(comando.Slug), comando.Descricao);

        try
        {
            await repositorio.AddAsync(categoria, ct);
        }
        catch (SlugEmUsoException)
        {
            return SlugEmUso(comando.Slug);
        }

        return Result.Ok(CategoriaResponse.De(categoria));
    }

    private static Result<CategoriaResponse> SlugEmUso(string slug) =>
        Result.Falha<CategoriaResponse>(
            Error.Conflito("slug_em_uso", $"Ja existe uma categoria com o slug '{slug}'."));
}
