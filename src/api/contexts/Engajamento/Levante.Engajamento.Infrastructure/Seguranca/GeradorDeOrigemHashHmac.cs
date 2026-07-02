using System.Security.Cryptography;
using System.Text;
using Levante.Engajamento.Application.Ports;
using Microsoft.Extensions.Options;

namespace Levante.Engajamento.Infrastructure.Seguranca;

/// <summary>
/// Pseudonimiza a origem (IP + User-Agent) com HMAC-SHA256 e segredo do servidor.
/// Irreversivel e estavel: a mesma origem gera o mesmo hash (dedup), mas o IP cru
/// nunca e derivavel sem o segredo (LGPD). O IP nunca e persistido nem logado.
/// </summary>
internal sealed class GeradorDeOrigemHashHmac : IGeradorDeOrigemHash
{
    private readonly byte[] _chave;

    public GeradorDeOrigemHashHmac(IOptions<EngajamentoOptions> options)
    {
        ArgumentNullException.ThrowIfNull(options);
        _chave = Encoding.UTF8.GetBytes(options.Value.OrigemHashSecret);
    }

    public string Gerar(string ip, string userAgent)
    {
        var origem = $"{ip}|{userAgent}";
        var hash = HMACSHA256.HashData(_chave, Encoding.UTF8.GetBytes(origem));
        return Convert.ToHexStringLower(hash);
    }
}
