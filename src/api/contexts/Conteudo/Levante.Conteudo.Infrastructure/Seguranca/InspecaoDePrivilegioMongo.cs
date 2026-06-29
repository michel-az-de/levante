using MongoDB.Bson;
using MongoDB.Driver;

namespace Levante.Conteudo.Infrastructure.Seguranca;

/// <summary>Papel atribuido a um usuario do Mongo (role + database).</summary>
public sealed record PapelMongo(string Role, string Db);

/// <summary>Privilegio efetivo: recurso (anyResource?) + acoes concedidas.</summary>
public sealed record PrivilegioMongo(bool AnyResource, IReadOnlyList<string> Actions);

/// <summary>Snapshot dos papeis/privilegios da conexao Mongo autenticada.</summary>
public sealed record StatusConexaoMongo(
    IReadOnlyList<PapelMongo> Papeis,
    IReadOnlyList<PrivilegioMongo> Privilegios);

/// <summary>
/// Deteccao de privilegio administrativo na conta de runtime do MongoDB.
/// Codigo de PRODUCAO: usado pelo self-check de boot e validado pelos testes.
/// Regra de seguranca nao-negociavel (CLAUDE.md, secao Seguranca).
/// </summary>
public static class InspecaoDePrivilegioMongo
{
    private static readonly HashSet<string> PapeisAdministrativos = new(StringComparer.OrdinalIgnoreCase)
    {
        "root",
        "dbOwner",
        "userAdmin",
        "userAdminAnyDatabase",
        "dbAdmin",
        "dbAdminAnyDatabase",
        "clusterAdmin",
        "clusterManager",
        "clusterMonitor",
        "hostManager",
        "readWriteAnyDatabase",
        "readAnyDatabase",
        "backup",
        "restore",
        "atlasAdmin",
    };

    private static readonly HashSet<string> AcoesPerigosas = new(StringComparer.OrdinalIgnoreCase)
    {
        "anyAction",
        "createUser",
        "dropUser",
        "updateUser",
        "grantRole",
        "revokeRole",
        "createRole",
        "dropRole",
        "dropDatabase",
        "shutdown",
        "addShard",
        "removeShard",
    };

    /// <summary>True se os papeis/privilegios indicarem qualquer privilegio administrativo.</summary>
    public static bool EhPrivilegioAdministrativo(
        IEnumerable<PapelMongo> papeis,
        IEnumerable<PrivilegioMongo> privilegios)
    {
        ArgumentNullException.ThrowIfNull(papeis);
        ArgumentNullException.ThrowIfNull(privilegios);

        if (papeis.Any(p => PapeisAdministrativos.Contains(p.Role)))
        {
            return true;
        }

        foreach (var privilegio in privilegios)
        {
            if (privilegio.AnyResource || privilegio.Actions.Any(AcoesPerigosas.Contains))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Le os papeis/privilegios da conexao atual via <c>connectionStatus</c>
    /// (com <c>showPrivileges</c>). Operacao permitida a qualquer usuario autenticado.
    /// </summary>
    public static async Task<StatusConexaoMongo> LerStatusDaConexaoAsync(
        IMongoDatabase database,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(database);

        var comando = new BsonDocument
        {
            { "connectionStatus", 1 },
            { "showPrivileges", true },
        };

        var resultado = await database.RunCommandAsync<BsonDocument>(comando, cancellationToken: ct);
        var authInfo = resultado.GetValue("authInfo", new BsonDocument()).AsBsonDocument;

        return new StatusConexaoMongo(
            ExtrairPapeis(authInfo),
            ExtrairPrivilegios(authInfo));
    }

    private static List<PapelMongo> ExtrairPapeis(BsonDocument authInfo)
    {
        if (!authInfo.TryGetValue("authenticatedUserRoles", out var valor) || !valor.IsBsonArray)
        {
            return [];
        }

        return valor.AsBsonArray
            .Select(item => item.AsBsonDocument)
            .Select(doc => new PapelMongo(
                doc.GetValue("role", BsonString.Empty).AsString,
                doc.GetValue("db", BsonString.Empty).AsString))
            .ToList();
    }

    private static List<PrivilegioMongo> ExtrairPrivilegios(BsonDocument authInfo)
    {
        if (!authInfo.TryGetValue("authenticatedUserPrivileges", out var valor) || !valor.IsBsonArray)
        {
            return [];
        }

        return valor.AsBsonArray
            .Select(item => item.AsBsonDocument)
            .Select(MapearPrivilegio)
            .ToList();
    }

    private static PrivilegioMongo MapearPrivilegio(BsonDocument doc)
    {
        var recurso = doc.GetValue("resource", new BsonDocument()).AsBsonDocument;
        var anyResource = recurso.GetValue("anyResource", BsonBoolean.False).ToBoolean();
        var acoes = doc.GetValue("actions", new BsonArray()).AsBsonArray
            .Select(a => a.AsString)
            .ToList();

        return new PrivilegioMongo(anyResource, acoes);
    }
}
