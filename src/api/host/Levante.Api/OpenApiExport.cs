using System.Diagnostics.CodeAnalysis;
using System.Text;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace Levante.Api;

/// <summary>
/// Emite o documento OpenAPI para um arquivo SEM subir o host (sem hosted
/// services, sem tocar o Mongo). Alimenta o contrato versionado em
/// src/web/openapi/levante.json e o gate de sincronia do CI.
/// </summary>
internal static class OpenApiExport
{
    public const string Argumento = "--emit-openapi";
    public const string NomeDocumento = "v1";

    [SuppressMessage(
        "Major Code Smell",
        "S1075:URIs should not be hardcoded",
        Justification = "Bind em loopback/porta efemera, usado apenas para emitir o contrato OpenAPI.")]
    public const string UrlEfemera = "http://127.0.0.1:0";

    public static async Task EmitAndExitAsync(WebApplication app, string caminho)
    {
        // Sobe o host em porta efemera para registrar os endpoints no
        // EndpointDataSource. Em modo emit nao ha hosted services nem
        // ValidateOnStart, entao o Mongo nao e tocado.
        await app.StartAsync();
        try
        {
            var provider = app.Services.GetRequiredKeyedService<IOpenApiDocumentProvider>(NomeDocumento);
            var documento = await provider.GetOpenApiDocumentAsync();

            using var memoria = new MemoryStream();
            await documento.SerializeAsJsonAsync(memoria, OpenApiSpecVersion.OpenApi3_0);
            var json = Encoding.UTF8.GetString(memoria.ToArray());

            // Determinismo cross-platform: normaliza fim de linha estrutural e
            // escapado (descricoes vindas de XML doc) para LF, garantindo bytes
            // identicos no Windows e no Ubuntu (gate de sincronia do CI).
            json = json
                .Replace("\r\n", "\n", StringComparison.Ordinal)
                .Replace("\\r\\n", "\\n", StringComparison.Ordinal);
            if (!json.EndsWith('\n'))
            {
                json += "\n";
            }

            var diretorio = Path.GetDirectoryName(Path.GetFullPath(caminho));
            if (!string.IsNullOrEmpty(diretorio))
            {
                Directory.CreateDirectory(diretorio);
            }

            await File.WriteAllTextAsync(caminho, json, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        }
        finally
        {
            await app.StopAsync();
        }
    }
}
