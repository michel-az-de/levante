using Levante.Conteudo.Domain.Artigos;
using Levante.SharedKernel;

namespace Levante.Conteudo.Application.Artigos.PublicarArtigo;

/// <summary>
/// Publica um artigo (handler direto, GAP-F). Arquivado e terminal: publicar e falha
/// de negocio (transicao_invalida). Publicar.Publicado e no-op idempotente.
/// </summary>
public sealed class PublicarArtigoCommandHandler(IArtigoRepository repositorio)
{
    public async Task<Result<ArtigoResponse>> Handle(PublicarArtigoCommand comando, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(comando);

        var artigo = await repositorio.GetByIdAsync(comando.Id, ct);
        if (artigo is null)
        {
            return Result.Falha<ArtigoResponse>(
                new Error("artigo_nao_encontrado", $"Artigo '{comando.Id}' nao encontrado."));
        }

        if (artigo.Status == StatusArtigo.Arquivado)
        {
            return Result.Falha<ArtigoResponse>(
                new Error("transicao_invalida", "Artigo arquivado nao pode ser publicado."));
        }

        artigo.Publicar();
        await repositorio.UpdateAsync(artigo, ct);

        return Result.Ok(ArtigoResponse.DeArtigo(artigo));
    }
}
