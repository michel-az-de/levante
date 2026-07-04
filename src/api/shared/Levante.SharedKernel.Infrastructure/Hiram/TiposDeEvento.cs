namespace Levante.SharedKernel.Infrastructure.Hiram;

/// <summary>
/// Vocabulario canonico (snake_case) dos eventos do Levante no Hiram. Sao tipos
/// PROPRIOS do tenant Levante (resolvidos por Routine no Hiram), nao os do EasyStok.
/// Uma unica fonte da string do fio; nunca trafegar ordinal.
/// </summary>
internal static class TiposDeEvento
{
    public const string AssinaturaSolicitada = "assinatura_solicitada";
    public const string AssinanteConfirmado = "assinante_confirmado";
    public const string ComentarioPendente = "comentario_pendente";
}
