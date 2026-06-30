using Microsoft.Extensions.Logging;

namespace Levante.Conteudo.Infrastructure;

/// <summary>
/// Logs de alta performance (source-generated) do contexto Conteudo.
/// Evita avaliacao de argumentos quando o nivel esta desabilitado (CA1873).
/// </summary>
internal static partial class LogConteudo
{
    [LoggerMessage(Level = LogLevel.Information, Message = "Seed de artigos inserido ({Quantidade}).")]
    public static partial void SeedInserido(ILogger logger, int quantidade);

    [LoggerMessage(Level = LogLevel.Information, Message = "Seed de artigos ja presente; duplicatas ignoradas.")]
    public static partial void SeedJaPresente(ILogger logger, Exception excecao);

    [LoggerMessage(Level = LogLevel.Information, Message = "Seed de categorias inserido ({Quantidade}).")]
    public static partial void SeedCategoriasInserido(ILogger logger, int quantidade);

    [LoggerMessage(Level = LogLevel.Information, Message = "Seed de categorias ja presente; duplicatas ignoradas.")]
    public static partial void SeedCategoriasJaPresente(ILogger logger, Exception excecao);

    [LoggerMessage(
        Level = LogLevel.Critical,
        Message = "A conta de runtime do MongoDB possui privilegio administrativo (papeis: {Papeis}). " +
                  "Viola o principio de privilegio minimo.")]
    public static partial void PrivilegioAdministrativoDetectado(ILogger logger, string papeis);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Self-check de privilegio do MongoDB OK: conta de runtime sem papel administrativo.")]
    public static partial void SelfCheckOk(ILogger logger);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Nao foi possivel executar o self-check de privilegio do MongoDB.")]
    public static partial void SelfCheckFalhou(ILogger logger, Exception excecao);
}
