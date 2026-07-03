using Levante.SharedKernel;

namespace Levante.Audiencia.Domain.Assinantes;

/// <summary>Fato: o assinante cancelou a assinatura (opt-out). Outbox -> Hiram para de enviar.</summary>
public sealed record AssinaturaCancelada(Guid AssinanteId, DateTime DataCancelamento) : IEventoDeDominio;
