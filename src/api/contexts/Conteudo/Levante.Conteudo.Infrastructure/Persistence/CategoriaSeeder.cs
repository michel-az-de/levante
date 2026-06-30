using Levante.Conteudo.Domain.Artigos;
using Levante.Conteudo.Domain.Categorias;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace Levante.Conteudo.Infrastructure.Persistence;

/// <summary>
/// Seed de categorias (Development/testes). Idempotente e seguro sob scale-out:
/// tolera duplicate-key (indice unico de slug).
/// </summary>
internal static class CategoriaSeeder
{
    public static IReadOnlyList<Categoria> Sementes() =>
    [
        Categoria.Criar("Arquitetura", new Slug("arquitetura"), "Decisoes de arquitetura, trade-offs e padroes."),
        Categoria.Criar("DevOps", new Slug("devops"), "Esteira, CI/CD, observabilidade e operacao."),
    ];

    public static async Task SeedAsync(ConteudoMongoContext contexto, ILogger logger, CancellationToken ct)
    {
        var documentos = Sementes().Select(CategoriaDocument.DeDominio).ToList();

        try
        {
            await contexto.Categorias.InsertManyAsync(
                documentos,
                new InsertManyOptions { IsOrdered = false },
                ct);

            LogConteudo.SeedCategoriasInserido(logger, documentos.Count);
        }
        catch (MongoBulkWriteException<CategoriaDocument> ex)
            when (ex.WriteErrors.All(e => e.Category == ServerErrorCategory.DuplicateKey))
        {
            LogConteudo.SeedCategoriasJaPresente(logger, ex);
        }
    }
}
