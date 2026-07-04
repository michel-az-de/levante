namespace Levante.SharedKernel.Infrastructure.Outbox;

/// <summary>
/// Estado de uma linha do Outbox no fluxo de emissao para o Hiram (relay flag-based).
/// Persistido como int no documento. <see cref="Ignorada"/> existe para nunca marcar
/// <see cref="Enviada"/> um evento sem mapeador (honestidade da trilha de auditoria).
/// </summary>
internal enum StatusEmissao
{
    Pendente = 0,
    Enviada = 1,
    Falhada = 2,
    Ignorada = 3,
}
