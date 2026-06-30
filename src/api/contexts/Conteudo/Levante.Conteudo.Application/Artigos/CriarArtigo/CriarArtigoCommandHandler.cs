using FluentValidation;
using Levante.Conteudo.Domain.Artigos;
using Levante.SharedKernel;

namespace Levante.Conteudo.Application.Artigos.CriarArtigo;

/// <summary>
/// Cria um artigo (handler chamado direto, GAP-F). Valida via FluentValidation e
/// pre-checa a unicidade do slug por Result (indice unico e a rede de seguranca).
/// </summary>
public sealed class CriarArtigoCommandHandler(
    IArtigoRepository repositorio,
    IValidator<CriarArtigoCommand> validador)
{
    public async Task<Result<ArtigoResponse>> Handle(CriarArtigoCommand comando, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(comando);

        var validacao = await validador.ValidateAsync(comando, ct);
        if (!validacao.IsValid)
        {
            return Result.Falha<ArtigoResponse>(ErroDeValidacao.De(validacao));
        }

        var existente = await repositorio.GetBySlugAsync(comando.Slug, ct);
        if (existente is not null)
        {
            return SlugEmUso(comando.Slug);
        }

        var artigo = Artigo.Criar(comando.Titulo, new Slug(comando.Slug), comando.Resumo, comando.Conteudo);

        try
        {
            await repositorio.AddAsync(artigo, ct);
        }
        catch (SlugEmUsoException)
        {
            // Corrida: o slug foi inserido entre a pre-checagem e o insert (indice unico).
            return SlugEmUso(comando.Slug);
        }

        return Result.Ok(ArtigoResponse.DeArtigo(artigo));
    }

    private static Result<ArtigoResponse> SlugEmUso(string slug) =>
        Result.Falha<ArtigoResponse>(
            new Error("slug_em_uso", $"Ja existe um artigo com o slug '{slug}'."));
}
