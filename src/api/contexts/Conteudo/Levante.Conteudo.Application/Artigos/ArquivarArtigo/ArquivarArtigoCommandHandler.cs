using Levante.Conteudo.Domain.Artigos;
using Levante.SharedKernel;

namespace Levante.Conteudo.Application.Artigos.ArquivarArtigo;

/// <summary>
/// Arquiva um artigo (handler direto, GAP-F). Estado terminal e idempotente: arquivar
/// um ja arquivado e no-op. E a acao "despublicar" (some do publico).
/// </summary>
public sealed class ArquivarArtigoCommandHandler(IArtigoRepository repositorio)
{
    public async Task<Result<ArtigoResponse>> Handle(ArquivarArtigoCommand comando, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(comando);

        var artigo = await repositorio.GetByIdAsync(comando.Id, ct);
        if (artigo is null)
        {
            return Result.Falha<ArtigoResponse>(
                Error.NaoEncontrado("artigo_nao_encontrado", $"Artigo '{comando.Id}' nao encontrado."));
        }

        artigo.Arquivar();
        await repositorio.UpdateAsync(artigo, ct);

        return Result.Ok(ArtigoResponse.DeArtigo(artigo));
    }
}
