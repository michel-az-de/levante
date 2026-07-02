using Levante.SharedKernel;

namespace Levante.Engajamento.Domain.Comentarios;

/// <summary>
/// Fato: um comentario foi criado e aguarda moderacao. Vira notificacao ao admin
/// via Outbox -> Hiram (Fase C). Hoje e levantado e limpo (ainda sem Outbox).
/// </summary>
public sealed record ComentarioCriado(Guid ComentarioId, Guid ArtigoId, DateTime DataCriacao) : IEventoDeDominio;
