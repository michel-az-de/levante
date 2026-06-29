using Microsoft.Extensions.Logging;

namespace Levante.Identity.Infrastructure;

/// <summary>Logs de alta performance (source-generated) do contexto Identity.</summary>
internal static partial class LogIdentity
{
    [LoggerMessage(Level = LogLevel.Information, Message = "Administrador semeado ({Email}).")]
    public static partial void AdminSemeado(ILogger logger, string email);
}
