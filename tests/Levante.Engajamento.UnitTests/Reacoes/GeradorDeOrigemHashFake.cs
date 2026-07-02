using Levante.Engajamento.Application.Ports;

namespace Levante.Engajamento.UnitTests.Reacoes;

/// <summary>Fake deterministico do gerador de hash (nao precisa de segredo real nos testes).</summary>
internal sealed class GeradorDeOrigemHashFake : IGeradorDeOrigemHash
{
    public string Gerar(string ip, string userAgent) => $"hash:{ip}|{userAgent}";
}
