using Levante.Conteudo.Domain.Artigos;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace Levante.Conteudo.Infrastructure.Persistence;

/// <summary>
/// Seed do walking skeleton. Roda apenas em Development/testes (ver chamador).
/// Idempotente e seguro sob scale-out: tolera duplicate-key (indice unico de slug).
/// </summary>
internal static class ArtigoSeeder
{
    public static IReadOnlyList<Artigo> Sementes()
    {
        var primeiro = Artigo.Criar(
            "Clean Architecture na pratica",
            new Slug("clean-architecture-na-pratica"),
            "Walking skeleton do Levante: do navegador ao Mongo, atravessando todas as camadas.");
        primeiro.Publicar();

        var segundo = Artigo.Criar(
            "Da pedra bruta a pedra polida",
            new Slug("da-pedra-bruta-a-pedra-polida"),
            "A esteira rough-cut, dress, polish e raise que leva o codigo a producao.");
        segundo.Publicar();

        return [primeiro, segundo];
    }

    public static async Task SeedAsync(ConteudoMongoContext contexto, ILogger logger, CancellationToken ct)
    {
        var documentos = Sementes().Select(ArtigoDocument.DeDominio).ToList();

        try
        {
            await contexto.Artigos.InsertManyAsync(
                documentos,
                new InsertManyOptions { IsOrdered = false },
                ct);

            LogConteudo.SeedInserido(logger, documentos.Count);
        }
        catch (MongoBulkWriteException<ArtigoDocument> ex)
            when (ex.WriteErrors.All(e => e.Category == ServerErrorCategory.DuplicateKey))
        {
            LogConteudo.SeedJaPresente(logger, ex);
        }
    }
}
