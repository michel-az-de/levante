using Levante.SharedKernel.Infrastructure.Outbox;

namespace Levante.SharedKernel.Infrastructure.Hiram;

/// <summary>Resultado do mapeamento: os campos que compoem o <see cref="HiramEventRequest"/>.</summary>
internal sealed record EmissaoHiram(
    string EventType,
    string? RecipientEmail,
    IReadOnlyDictionary<string, object?> Data);

/// <summary>
/// Traduz um evento de dominio (linha de outbox) no evento de integracao do Hiram
/// (a ideia de <c>...IntegrationEvent</c> curado da ADR). Curadoria fica na
/// Infrastructure — o dominio nao conhece o contrato do Hiram.
/// </summary>
internal interface IMapeadorDeEmissao
{
    /// <summary>
    /// True + <paramref name="emissao"/> quando ha mapeamento para o <c>doc.Tipo</c>;
    /// false quando o evento nao vira notificacao (o relay marca <c>Ignorada</c>).
    /// </summary>
    bool TryMapear(OutboxDocument doc, out EmissaoHiram emissao);
}
