using Levante.Conteudo.Domain.Midias;
using Levante.SharedKernel;

namespace Levante.Conteudo.Application.Midias.ObterMidia;

public sealed class ObterMidiaQueryHandler(IMidiaStorage armazenamento)
{
    public async Task<Result<MidiaConteudo>> Handle(ObterMidiaQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        var midia = await armazenamento.AbrirAsync(query.Id, ct);

        return midia is null
            ? Result.Falha<MidiaConteudo>(Error.NaoEncontrado("midia_nao_encontrada", $"Midia '{query.Id}' nao encontrada."))
            : Result.Ok(midia);
    }
}
