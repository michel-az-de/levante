using Levante.SharedKernel.Infrastructure.Outbox;
using Microsoft.Extensions.Options;

namespace Levante.SharedKernel.Infrastructure.Hiram;

/// <summary>
/// Mapeamento curado dos 3 eventos do Levante v1. Le <c>doc.Tipo</c> (nome do fato de
/// dominio) por string — o shared kernel nao referencia os dominios de contexto — e
/// extrai os campos de <c>doc.Dados</c> (camelCase). LGPD-minimo: so o necessario.
/// </summary>
internal sealed class MapeadorDeEmissao(IOptions<NotificacoesOptions> opcoes) : IMapeadorDeEmissao
{
    private const string EventoAssinaturaSolicitada = "AssinaturaSolicitada";
    private const string EventoAssinanteConfirmado = "AssinanteConfirmado";
    private const string EventoComentarioCriado = "ComentarioCriado";

    private readonly NotificacoesOptions _opcoes = opcoes.Value;

    public bool TryMapear(OutboxDocument doc, out EmissaoHiram emissao)
    {
        ArgumentNullException.ThrowIfNull(doc);

        switch (doc.Tipo)
        {
            case EventoAssinaturaSolicitada:
                emissao = new EmissaoHiram(
                    TiposDeEvento.AssinaturaSolicitada,
                    Campo(doc, "email"),
                    new Dictionary<string, object?>(StringComparer.Ordinal)
                    {
                        ["token"] = Campo(doc, "token"),
                        ["confirmUrlBase"] = $"{_opcoes.SiteUrl.TrimEnd('/')}/newsletter/confirmar",
                    });
                return true;

            case EventoAssinanteConfirmado:
                emissao = new EmissaoHiram(
                    TiposDeEvento.AssinanteConfirmado,
                    Campo(doc, "email"),
                    new Dictionary<string, object?>(StringComparer.Ordinal));
                return true;

            case EventoComentarioCriado:
                emissao = new EmissaoHiram(
                    TiposDeEvento.ComentarioPendente,
                    _opcoes.AdminEmail,
                    new Dictionary<string, object?>(StringComparer.Ordinal)
                    {
                        ["comentarioId"] = Campo(doc, "comentarioId"),
                        ["artigoId"] = Campo(doc, "artigoId"),
                        ["dataCriacao"] = Campo(doc, "dataCriacao"),
                    });
                return true;

            default:
                emissao = null!;
                return false;
        }
    }

    private static string? Campo(OutboxDocument doc, string nome) =>
        doc.Dados.TryGetValue(nome, out var valor) && !valor.IsBsonNull ? valor.ToString() : null;
}
