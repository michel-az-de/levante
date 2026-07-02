using Levante.SharedKernel;

namespace Levante.Engajamento.Domain.Comentarios;

/// <summary>
/// Agregado de comentario a um artigo. Nasce <see cref="StatusComentario.Pendente"/>
/// e so aparece no publico apos moderacao (Aprovar). Sem e-mail: guarda apenas
/// nome de exibicao e texto (superficie LGPD minima). <see cref="Visitante"/> e id
/// opaco (cookie) e <see cref="OrigemHash"/> e HMAC de IP+UA (IP cru nunca guardado).
/// </summary>
public sealed class Comentario
{
    private readonly List<IEventoDeDominio> _eventos = [];

    public const int TamanhoMaximoAutor = 60;

    private Comentario(
        Guid id,
        Guid artigoId,
        string artigoSlug,
        string autor,
        TextoComentario texto,
        StatusComentario status,
        string visitante,
        string origemHash,
        DateTime dataCriacao,
        DateTime? dataModeracao)
    {
        Id = id;
        ArtigoId = artigoId;
        ArtigoSlug = artigoSlug;
        Autor = autor;
        Texto = texto;
        Status = status;
        Visitante = visitante;
        OrigemHash = origemHash;
        DataCriacao = dataCriacao;
        DataModeracao = dataModeracao;
    }

    public Guid Id { get; }

    public Guid ArtigoId { get; }

    /// <summary>Slug do artigo, denormalizado para o admin linkar sem cruzar contexto.</summary>
    public string ArtigoSlug { get; }

    public string Autor { get; }

    public TextoComentario Texto { get; }

    public StatusComentario Status { get; private set; }

    public string Visitante { get; }

    public string OrigemHash { get; }

    public DateTime DataCriacao { get; }

    public DateTime? DataModeracao { get; private set; }

    public IReadOnlyList<IEventoDeDominio> Eventos => _eventos;

    /// <summary>Cria um comentario pendente e registra o evento de dominio.</summary>
    public static Comentario Criar(
        Guid artigoId, string artigoSlug, string autor, TextoComentario texto, string visitante, string origemHash)
    {
        GarantirArtigo(artigoId);
        ArgumentException.ThrowIfNullOrWhiteSpace(artigoSlug);
        GarantirAutor(autor);
        ArgumentNullException.ThrowIfNull(texto);
        ArgumentException.ThrowIfNullOrWhiteSpace(visitante);
        ArgumentNullException.ThrowIfNull(origemHash);

        var comentario = new Comentario(
            Guid.NewGuid(),
            artigoId,
            artigoSlug.Trim(),
            autor.Trim(),
            texto,
            StatusComentario.Pendente,
            visitante,
            origemHash,
            DateTime.UtcNow,
            dataModeracao: null);

        comentario._eventos.Add(new ComentarioCriado(comentario.Id, artigoId, comentario.DataCriacao));
        return comentario;
    }

    /// <summary>Rehidrata um comentario existente (uso da camada de persistencia).</summary>
    public static Comentario Reconstituir(
        Guid id,
        Guid artigoId,
        string artigoSlug,
        string autor,
        TextoComentario texto,
        StatusComentario status,
        string visitante,
        string origemHash,
        DateTime dataCriacao,
        DateTime? dataModeracao) =>
        new(id, artigoId, artigoSlug, autor, texto, status, visitante, origemHash, dataCriacao, dataModeracao);

    /// <summary>Aprova o comentario (idempotente) e registra o evento de dominio.</summary>
    public void Aprovar()
    {
        if (Status == StatusComentario.Aprovado)
        {
            return;
        }

        Status = StatusComentario.Aprovado;
        DataModeracao = DateTime.UtcNow;
        _eventos.Add(new ComentarioAprovado(Id, ArtigoId, DataModeracao.Value));
    }

    /// <summary>Rejeita o comentario (idempotente; estado terminal de moderacao).</summary>
    public void Rejeitar()
    {
        if (Status == StatusComentario.Rejeitado)
        {
            return;
        }

        Status = StatusComentario.Rejeitado;
        DataModeracao = DateTime.UtcNow;
    }

    /// <summary>Limpa eventos ja despachados (chamado apos persistir no Outbox).</summary>
    public void LimparEventos() => _eventos.Clear();

    private static void GarantirArtigo(Guid artigoId)
    {
        if (artigoId == Guid.Empty)
        {
            throw new ArgumentException("ArtigoId nao pode ser vazio.", nameof(artigoId));
        }
    }

    private static void GarantirAutor(string autor)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(autor);

        if (autor.Trim().Length > TamanhoMaximoAutor)
        {
            throw new ArgumentException($"Autor excede {TamanhoMaximoAutor} caracteres.", nameof(autor));
        }
    }
}
