using Levante.Conteudo.Domain.Midias;

namespace Levante.Conteudo.UnitTests.Midias;

/// <summary>Armazenamento de midia em memoria para testar handlers sem Mongo/GridFS (unit).</summary>
internal sealed class MidiaStorageEmMemoria : IMidiaStorage
{
    private readonly Dictionary<Guid, (byte[] Bytes, string ContentType)> _midias = [];

    public int Salvas { get; private set; }

    public string? UltimoContentType { get; private set; }

    /// <summary>Bytes gravados no ultimo SalvarAsync (o real: le a partir da posicao atual do stream).</summary>
    public byte[] UltimoConteudo { get; private set; } = [];

    public Task<MidiaArmazenada> SalvarAsync(
        Guid id, Stream conteudo, string contentType, string nomeArquivo, CancellationToken ct)
    {
        // Copia a partir da posicao ATUAL, como o GridFS faz: um stream entregue
        // ja consumido grava 0 byte, e o teste precisa enxergar isso.
        using var memoria = new MemoryStream();
        conteudo.CopyTo(memoria);
        var bytes = memoria.ToArray();

        _midias[id] = (bytes, contentType);
        Salvas++;
        UltimoContentType = contentType;
        UltimoConteudo = bytes;

        return Task.FromResult(new MidiaArmazenada(id, contentType, bytes.Length));
    }

    public Task<MidiaConteudo?> AbrirAsync(Guid id, CancellationToken ct)
    {
        if (!_midias.TryGetValue(id, out var midia))
        {
            return Task.FromResult<MidiaConteudo?>(null);
        }

        // Devolve o content-type REAL do que foi salvo; devolver um fixo esconderia
        // qualquer bug de normalizacao de content-type.
        return Task.FromResult<MidiaConteudo?>(
            new MidiaConteudo(new MemoryStream(midia.Bytes), midia.ContentType, midia.Bytes.Length));
    }

    public Task<bool> RemoverAsync(Guid id, CancellationToken ct) => Task.FromResult(_midias.Remove(id));
}
