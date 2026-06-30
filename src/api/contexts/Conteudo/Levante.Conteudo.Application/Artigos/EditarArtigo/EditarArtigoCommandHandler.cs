using FluentValidation;
using Levante.Conteudo.Domain.Artigos;
using Levante.SharedKernel;

namespace Levante.Conteudo.Application.Artigos.EditarArtigo;

/// <summary>
/// Edita um artigo (handler direto, GAP-F). Artigo arquivado e terminal: editar e
/// falha de negocio (transicao_invalida). Slug duplicado em outro artigo => slug_em_uso.
/// </summary>
public sealed class EditarArtigoCommandHandler(
    IArtigoRepository repositorio,
    IValidator<EditarArtigoCommand> validador)
{
    public async Task<Result<ArtigoResponse>> Handle(EditarArtigoCommand comando, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(comando);

        var validacao = await validador.ValidateAsync(comando, ct);
        if (!validacao.IsValid)
        {
            return Result.Falha<ArtigoResponse>(ErroDeValidacao.De(validacao));
        }

        var artigo = await repositorio.GetByIdAsync(comando.Id, ct);
        if (artigo is null)
        {
            return Result.Falha<ArtigoResponse>(
                new Error("artigo_nao_encontrado", $"Artigo '{comando.Id}' nao encontrado."));
        }

        if (artigo.Status == StatusArtigo.Arquivado)
        {
            return Result.Falha<ArtigoResponse>(
                new Error("transicao_invalida", "Artigo arquivado nao pode ser editado."));
        }

        var conflito = await repositorio.GetBySlugAsync(comando.Slug, ct);
        if (conflito is not null && conflito.Id != artigo.Id)
        {
            return SlugEmUso(comando.Slug);
        }

        var meta = MetaSeo.Criar(comando.MetaTitulo, comando.MetaDescricao, comando.ImagemOgUrl);
        var tags = TagsDoComando.Converter(comando.Tags);
        artigo.Editar(
            comando.Titulo, new Slug(comando.Slug), comando.Resumo, comando.Conteudo, meta, comando.CategoriaId, tags);

        try
        {
            await repositorio.UpdateAsync(artigo, ct);
        }
        catch (SlugEmUsoException)
        {
            // Corrida: outro artigo assumiu este slug entre a pre-checagem e o replace.
            return SlugEmUso(comando.Slug);
        }

        return Result.Ok(ArtigoResponse.DeArtigo(artigo));
    }

    private static Result<ArtigoResponse> SlugEmUso(string slug) =>
        Result.Falha<ArtigoResponse>(
            new Error("slug_em_uso", $"Ja existe um artigo com o slug '{slug}'."));
}
