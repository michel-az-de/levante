using Levante.Conteudo.Domain.Midias;

namespace Levante.Conteudo.UnitTests.Midias;

/// <summary>Armazenamento de midia em memoria para testar handlers sem Mongo/GridFS (unit).</summary>
internal sealed class MidiaStorageEmMemoria : IMidiaStorage
{
    private readonly Dictionary<Guid, byte[]> _midias = [];

    public int Salvas { get; private set; }

    public string? UltimoContentType { get; private set; }

    public Task<MidiaArmazenada> SalvarAsync(
        Guid id, Stream conteudo, string contentType, string nomeArquivo, CancellationToken ct)
    {
        using var memoria = new MemoryStream();
        conteudo.CopyTo(memoria);
        _midias[id] = memoria.ToArray();
        Salvas++;
        UltimoContentType = contentType;

        return Task.FromResult(new MidiaArmazenada(id, contentType, memoria.Length));
    }

    public Task<MidiaConteudo?> AbrirAsync(Guid id, CancellationToken ct)
    {
        if (!_midias.TryGetValue(id, out var bytes))
        {
            return Task.FromResult<MidiaConteudo?>(null);
        }

        return Task.FromResult<MidiaConteudo?>(new MidiaConteudo(new MemoryStream(bytes), "image/png", bytes.Length));
    }
}
