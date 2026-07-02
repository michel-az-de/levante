namespace Levante.Engajamento.Application.Ports;

/// <summary>
/// Porta que pseudonimiza a origem (IP + User-Agent) num hash irreversivel com
/// segredo do servidor. O IP cru nunca sai da borda nem e persistido (LGPD).
/// Implementada na Infrastructure (HMAC-SHA256).
/// </summary>
public interface IGeradorDeOrigemHash
{
    string Gerar(string ip, string userAgent);
}
