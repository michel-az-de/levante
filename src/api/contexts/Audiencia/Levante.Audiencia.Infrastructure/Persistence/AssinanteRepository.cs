using Levante.Audiencia.Domain.Assinantes;
using Levante.SharedKernel.Infrastructure.Outbox;
using MongoDB.Driver;

namespace Levante.Audiencia.Infrastructure.Persistence;

/// <summary>
/// Repositorio do agregado Assinante. As escritas passam pelo
/// <see cref="IGravadorDeAgregado"/>: os eventos (AssinaturaSolicitada/
/// AssinanteConfirmado/AssinaturaCancelada) vao ao Outbox na mesma transacao.
/// </summary>
internal sealed class AssinanteRepository(AudienciaMongoContext contexto, IGravadorDeAgregado gravador)
    : IAssinanteRepository
{
    public async Task AddAsync(Assinante assinante, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(assinante);

        var documento = AssinanteDocument.DeDominio(assinante);
        try
        {
            await gravador.ExecutarAsync(
                (sessao, c) => sessao is null
                    ? contexto.Assinantes.InsertOneAsync(documento, options: null, c)
                    : contexto.Assinantes.InsertOneAsync(sessao, documento, options: null, c),
                assinante.Eventos,
                ct);
        }
        catch (MongoException ex) when (EhChaveDuplicada(ex))
        {
            // Indice unico violado (e-mail ja cadastrado). A Application trata como idempotente.
            throw new AssinanteJaExisteException(assinante.Email.Valor);
        }

        assinante.LimparEventos();
    }

    // Detecta violacao de indice unico mesmo quando o driver embrulha a excecao numa
    // transacao (replica set/Atlas): a insercao roda dentro do gravador transacional, e
    // ai o DuplicateKey pode chegar aninhado. Codigo 11000 = duplicate key no servidor.
    private static bool EhChaveDuplicada(Exception? ex) => ex switch
    {
        MongoWriteException we => we.WriteError?.Category == ServerErrorCategory.DuplicateKey,
        MongoCommandException ce => ce.Code == 11000,
        null => false,
        _ => EhChaveDuplicada(ex.InnerException),
    };

    public async Task<Assinante?> GetByTokenAsync(string token, CancellationToken ct)
    {
        var doc = await contexto.Assinantes.Find(d => d.Token == token).FirstOrDefaultAsync(ct);
        return doc?.ParaDominio();
    }

    public async Task UpdateAsync(Assinante assinante, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(assinante);

        var documento = AssinanteDocument.DeDominio(assinante);
        await gravador.ExecutarAsync(
            (sessao, c) => sessao is null
                ? contexto.Assinantes.ReplaceOneAsync(d => d.Id == documento.Id, documento, new ReplaceOptions(), c)
                : contexto.Assinantes.ReplaceOneAsync(sessao, d => d.Id == documento.Id, documento, new ReplaceOptions(), c),
            assinante.Eventos,
            ct);

        assinante.LimparEventos();
    }
}
