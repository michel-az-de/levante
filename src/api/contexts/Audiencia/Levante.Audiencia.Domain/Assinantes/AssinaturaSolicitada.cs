using Levante.SharedKernel;

namespace Levante.Audiencia.Domain.Assinantes;

/// <summary>
/// Fato: um assinante solicitou a newsletter (double opt-in pendente). Vira evento
/// no Outbox -> Hiram, que envia o e-mail de confirmacao com o <paramref name="Token"/>.
/// O site nunca chama provedor de e-mail direto (CLAUDE.md, regra 7).
/// </summary>
public sealed record AssinaturaSolicitada(
    Guid AssinanteId, string Email, string Token, DateTime DataSolicitacao) : IEventoDeDominio;
