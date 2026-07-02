namespace Levante.Engajamento.Domain.Reacoes;

/// <summary>
/// Agregado de reacao de um visitante anonimo a um artigo. Unicidade e por
/// (<see cref="ArtigoId"/>, <see cref="Visitante"/>, <see cref="Tipo"/>): cada
/// visitante da no maximo uma reacao de cada tipo por artigo (indice unico na
/// Infrastructure).
///
/// Nao ha dado pessoal aqui: <see cref="Visitante"/> e um id opaco (cookie
/// first-party) e <see cref="OrigemHash"/> e um HMAC de IP+User-Agent com
/// segredo do servidor (o IP cru nunca e persistido). Ver LGPD no CLAUDE.md.
/// </summary>
public sealed class Reacao
{
    private Reacao(Guid id, Guid artigoId, TipoReacao tipo, string visitante, string origemHash, DateTime dataCriacao)
    {
        Id = id;
        ArtigoId = artigoId;
        Tipo = tipo;
        Visitante = visitante;
        OrigemHash = origemHash;
        DataCriacao = dataCriacao;
    }

    public Guid Id { get; }

    public Guid ArtigoId { get; }

    public TipoReacao Tipo { get; }

    /// <summary>Id opaco do visitante (cookie httpOnly first-party). Chave de unicidade.</summary>
    public string Visitante { get; }

    /// <summary>HMAC de IP+User-Agent (sinal anti-abuso secundario; nunca o IP cru).</summary>
    public string OrigemHash { get; }

    public DateTime DataCriacao { get; }

    /// <summary>Registra uma nova reacao do visitante ao artigo.</summary>
    public static Reacao Registrar(Guid artigoId, TipoReacao tipo, string visitante, string origemHash)
    {
        GarantirArtigo(artigoId);
        GarantirVisitante(visitante);
        ArgumentNullException.ThrowIfNull(origemHash);

        return new Reacao(Guid.NewGuid(), artigoId, tipo, visitante, origemHash, DateTime.UtcNow);
    }

    /// <summary>Rehidrata uma reacao existente (uso da camada de persistencia).</summary>
    public static Reacao Reconstituir(
        Guid id, Guid artigoId, TipoReacao tipo, string visitante, string origemHash, DateTime dataCriacao) =>
        new(id, artigoId, tipo, visitante, origemHash, dataCriacao);

    private static void GarantirArtigo(Guid artigoId)
    {
        if (artigoId == Guid.Empty)
        {
            throw new ArgumentException("ArtigoId nao pode ser vazio.", nameof(artigoId));
        }
    }

    private static void GarantirVisitante(string visitante) =>
        ArgumentException.ThrowIfNullOrWhiteSpace(visitante);
}
