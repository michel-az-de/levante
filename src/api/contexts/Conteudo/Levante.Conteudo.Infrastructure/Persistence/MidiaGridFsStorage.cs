using Levante.Conteudo.Domain.Midias;
using MongoDB.Bson;
using MongoDB.Driver.GridFS;

namespace Levante.Conteudo.Infrastructure.Persistence;

/// <summary>
/// Armazenamento de midia via GridFS, no mesmo Mongo do resto do contexto
/// (backup junto com o banco, sem storage novo). O content-type fica em
/// metadata.contentType: o campo ContentType nativo do GridFS e legado.
/// </summary>
internal sealed class MidiaGridFsStorage(ConteudoMongoContext contexto) : IMidiaStorage
{
    public async Task<MidiaArmazenada> SalvarAsync(
        Guid id, Stream conteudo, string contentType, string nomeArquivo, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(conteudo);

        var opcoes = new GridFSUploadOptions
        {
            Metadata = new BsonDocument { { "contentType", contentType } },
        };

        await contexto.Midias.UploadFromStreamAsync(id, nomeArquivo, conteudo, opcoes, ct);

        return new MidiaArmazenada(id, contentType, conteudo.Length);
    }

    public async Task<MidiaConteudo?> AbrirAsync(Guid id, CancellationToken ct)
    {
        try
        {
            var stream = await contexto.Midias.OpenDownloadStreamAsync(id, cancellationToken: ct);
            var contentType = stream.FileInfo.Metadata?.GetValue("contentType", null)?.AsString
                ?? "application/octet-stream";

            return new MidiaConteudo(stream, contentType, stream.FileInfo.Length);
        }
        catch (GridFSFileNotFoundException)
        {
            return null;
        }
    }
}
