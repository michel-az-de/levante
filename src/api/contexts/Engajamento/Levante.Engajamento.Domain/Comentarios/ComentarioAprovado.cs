using Levante.SharedKernel;

namespace Levante.Engajamento.Domain.Comentarios;

/// <summary>Fato: um comentario foi aprovado e agora e publico. Futuro gancho de Outbox (Fase C).</summary>
public sealed record ComentarioAprovado(Guid ComentarioId, Guid ArtigoId, DateTime DataAprovacao) : IEventoDeDominio;
