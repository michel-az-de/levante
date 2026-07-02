using Levante.Engajamento.Domain.Reacoes;

namespace Levante.Engajamento.UnitTests.Reacoes;

/// <summary>Fake em memoria de <see cref="IReacaoRepository"/> com contadores para assertions.</summary>
internal sealed class ReacaoRepositorioEmMemoria : IReacaoRepository
{
    private readonly List<Reacao> _reacoes;

    public ReacaoRepositorioEmMemoria(params Reacao[] reacoes) => _reacoes = [.. reacoes];

    public int Adicionadas { get; private set; }

    public int Removidas { get; private set; }

    public Task AddAsync(Reacao reacao, CancellationToken ct)
    {
        var duplicada = _reacoes.Exists(r =>
            r.ArtigoId == reacao.ArtigoId && r.Visitante == reacao.Visitante && r.Tipo == reacao.Tipo);
        if (duplicada)
        {
            throw new ReacaoDuplicadaException(reacao.ArtigoId, reacao.Tipo, reacao.Visitante);
        }

        _reacoes.Add(reacao);
        Adicionadas++;
        return Task.CompletedTask;
    }

    public Task RemoverAsync(Guid artigoId, TipoReacao tipo, string visitante, CancellationToken ct)
    {
        Removidas += _reacoes.RemoveAll(r =>
            r.ArtigoId == artigoId && r.Tipo == tipo && r.Visitante == visitante);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyDictionary<TipoReacao, int>> ContarPorArtigoAsync(Guid artigoId, CancellationToken ct)
    {
        IReadOnlyDictionary<TipoReacao, int> contagens = _reacoes
            .Where(r => r.ArtigoId == artigoId)
            .GroupBy(r => r.Tipo)
            .ToDictionary(g => g.Key, g => g.Count());

        return Task.FromResult(contagens);
    }

    public Task<IReadOnlyList<TipoReacao>> ListarTiposDoVisitanteAsync(
        Guid artigoId, string visitante, CancellationToken ct)
    {
        IReadOnlyList<TipoReacao> tipos =
        [
            .. _reacoes
                .Where(r => r.ArtigoId == artigoId && r.Visitante == visitante)
                .Select(r => r.Tipo),
        ];

        return Task.FromResult(tipos);
    }
}
