namespace Levante.Api.IntegrationTests.Fixtures;

/// <summary>
/// Imagens Docker fixadas dos containers de teste. Explicitar a imagem e
/// requisito do Testcontainers 4.12+ (o construtor sem parametro foi
/// deprecado) e evita surpresa quando o default do modulo mudar.
/// </summary>
internal static class ImagensDeTeste
{
    public const string Mongo = "mongo:8.0";
}
