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

        // Mede o que foi REALMENTE gravado (avanco do cursor) em vez de devolver
        // conteudo.Length: se o stream chegar posicionado no meio, o GridFS grava so
        // o resto, e reportar o tamanho total seria mentir sobre o que foi salvo.
        var posicaoInicial = conteudo.CanSeek ? conteudo.Position : 0L;
        await contexto.Midias.UploadFromStreamAsync(id, nomeArquivo, conteudo, opcoes, ct);
        var gravados = conteudo.CanSeek ? conteudo.Position - posicaoInicial : conteudo.Length;

        return new MidiaArmazenada(id, contentType, gravados);
    }

    public async Task<MidiaConteudo?> AbrirAsync(Guid id, CancellationToken ct)
    {
        try
        {
            // Seekable: o stream forward-only nao expoe Length, e sem isso a resposta
            // sai em chunked (sem Content-Length) e sem suporte a range.
            var opcoes = new GridFSDownloadOptions { Seekable = true };
            var stream = await contexto.Midias.OpenDownloadStreamAsync(id, opcoes, ct);
            var contentType = stream.FileInfo.Metadata?.GetValue("contentType", null)?.AsString
                ?? "application/octet-stream";

            return new MidiaConteudo(stream, contentType, stream.FileInfo.Length);
        }
        catch (GridFSFileNotFoundException)
        {
            return null;
        }
    }

    public async Task<bool> RemoverAsync(Guid id, CancellationToken ct)
    {
        try
        {
            await contexto.Midias.DeleteAsync(id, ct);
            return true;
        }
        catch (GridFSFileNotFoundException)
        {
            return false;
        }
    }
}
