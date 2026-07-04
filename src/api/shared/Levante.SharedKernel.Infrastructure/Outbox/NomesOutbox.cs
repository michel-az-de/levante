namespace Levante.SharedKernel.Infrastructure.Outbox;

/// <summary>Nomes tecnicos das collections do Outbox (EN, conforme convencao).</summary>
internal static class NomesOutbox
{
    /// <summary>
    /// Fila de emissoes. Gravada na mesma transacao do agregado. O relay le os
    /// pendentes por <c>emissionSeq</c> e MARCA (Enviada/Falhada/Ignorada) em vez de
    /// deletar — a linha vira trilha de auditoria de emissao (ADR 0002).
    /// </summary>
    public const string Collection = "outbox";

    /// <summary>
    /// Contadores monotonicos (Mongo nao tem bigserial). O doc <c>outbox_emission</c>
    /// guarda o proximo <c>emissionSeq</c>, incrementado via findOneAndUpdate($inc)
    /// dentro da transacao do gravador.
    /// </summary>
    public const string Sequencias = "sequencias";

    /// <summary>Chave do contador de <c>emissionSeq</c> na collection de sequencias.</summary>
    public const string ChaveEmissionSeq = "outbox_emission";
}
