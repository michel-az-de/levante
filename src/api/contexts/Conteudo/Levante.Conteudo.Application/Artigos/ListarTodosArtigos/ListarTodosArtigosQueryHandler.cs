using Levante.Conteudo.Domain.Artigos;
using Levante.SharedKernel;

namespace Levante.Conteudo.Application.Artigos.ListarTodosArtigos;

/// <summary>
/// Lista todos os artigos para o admin (qualquer status, ordem decrescente de criacao).
/// Handler chamado direto (GAP-F); usado so atras de autorizacao.
/// </summary>
public sealed class ListarTodosArtigosQueryHandler(IArtigoRepository repositorio)
{
    public async Task<Result<IReadOnlyList<ArtigoResponse>>> Handle(
        ListarTodosArtigosQuery query,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        var artigos = await repositorio.ListTodosAsync(ct);
        IReadOnlyList<ArtigoResponse> resposta = [.. artigos.Select(ArtigoResponse.DeArtigo)];

        return Result.Ok(resposta);
    }
}
