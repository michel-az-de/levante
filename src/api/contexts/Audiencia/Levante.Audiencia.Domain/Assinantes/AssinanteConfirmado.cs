using Levante.SharedKernel;

namespace Levante.Audiencia.Domain.Assinantes;

/// <summary>Fato: o assinante confirmou o double opt-in. Outbox -> Hiram (boas-vindas, opcional).</summary>
public sealed record AssinanteConfirmado(
    Guid AssinanteId, string Email, DateTime DataConfirmacao) : IEventoDeDominio;
