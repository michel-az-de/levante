namespace Levante.SharedKernel.Infrastructure.Hiram;

/// <summary>
/// Corpo do <c>POST /v1/events</c> do Hiram (contrato congelado, ver
/// <c>hiram/docs/contracts/v1-events.md</c> e <c>docs/adr/0002-emissao-hiram-http.md</c>).
/// Serializado em camelCase (JsonSerializerDefaults.Web). Um contract-test valida a
/// forma contra o snapshot do OpenAPI do Hiram (sem acoplamento de build cross-repo).
/// </summary>
public sealed record HiramEventRequest(
    string EventType,
    string EventId,
    long EmissionSeq,
    HiramRecipient Recipient,
    string? LogicalAlertId,
    string? Timezone,
    IReadOnlyDictionary<string, object?>? Data);

/// <summary>Contato no instante da emissao. Para os eventos do Levante v1, so <see cref="Email"/>.</summary>
public sealed record HiramRecipient(string? UserId, string? Email, string? Phone);
