using Levante.Conteudo.Domain.Midias;
using Levante.SharedKernel;

namespace Levante.Conteudo.Application.Midias.RemoverMidia;

/// <summary>
/// Remove a midia do armazenamento. Nao ha como recolher copias ja cacheadas pelo
/// browser/CDN (a leitura publica e servida com Cache-Control immutable), entao a
/// remocao vale para novos acessos, nao para quem ja baixou.
/// </summary>
public sealed class RemoverMidiaCommandHandler(IMidiaStorage armazenamento)
{
    public async Task<Result> Handle(RemoverMidiaCommand comando, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(comando);

        var removida = await armazenamento.RemoverAsync(comando.Id, ct);

        return removida
            ? Result.Ok()
            : Result.Falha(Error.NaoEncontrado("midia_nao_encontrada", $"Midia '{comando.Id}' nao encontrada."));
    }
}
