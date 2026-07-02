namespace Levante.SharedKernel.Infrastructure.Outbox;

/// <summary>Nome tecnico da collection do Outbox (EN, conforme convencao).</summary>
internal static class NomesOutbox
{
    /// <summary>
    /// Fila de eventos pendentes. O relay observa por Change Stream e DELETA cada
    /// evento apos publicar (o doc existir = ainda nao publicado). Um sweep no start
    /// recupera o que ficou de uma queda entre publicar e deletar (at-least-once).
    /// </summary>
    public const string Collection = "outbox";
}
