namespace Levante.SharedKernel.Infrastructure.Hiram;

/// <summary>
/// Dados que o mapeador de emissao precisa e nao vem do evento de dominio: o e-mail
/// do admin (destino do alerta de comentario pendente) e a URL base do site (para o
/// link de confirmacao da newsletter). Secao <c>Levante:Notificacoes</c>.
/// </summary>
public sealed class NotificacoesOptions
{
    public const string SecaoConfig = "Levante:Notificacoes";

    /// <summary>Destino do alerta <c>comentario_pendente</c> (moderacao).</summary>
    public string AdminEmail { get; set; } = string.Empty;

    /// <summary>URL base do site (sem barra final); compoe o link de confirmacao (GAP-A via env).</summary>
    public string SiteUrl { get; set; } = string.Empty;
}
