using System.Text.Json;
using Levante.SharedKernel.Infrastructure.Hiram;
using Shouldly;
using Xunit;

namespace Levante.Api.IntegrationTests.Contracts;

/// <summary>
/// Contract-test (unit, sem Docker): a forma serializada de <see cref="HiramEventRequest"/>
/// tem que casar com o contrato congelado do Hiram (snapshot de SubmitEventRequest/
/// EventRecipient em <c>hiram-events.contract.json</c>). Sem acoplamento de build
/// cross-repo — um drift no record OU no snapshot quebra aqui.
/// </summary>
public sealed class HiramContratoTests
{
    private static readonly JsonSerializerOptions Web = new(JsonSerializerDefaults.Web);

    [Fact]
    public void HiramEventRequest_serializa_conformeContratoCongelado()
    {
        var requisicao = new HiramEventRequest(
            EventType: "assinatura_solicitada",
            EventId: Guid.NewGuid().ToString(),
            EmissionSeq: 42,
            Recipient: new HiramRecipient(UserId: null, Email: "a@b.com", Phone: null),
            LogicalAlertId: null,
            Timezone: null,
            Data: new Dictionary<string, object?> { ["token"] = "t" });

        using var json = JsonDocument.Parse(JsonSerializer.Serialize(requisicao, Web));
        var contrato = CarregarContrato();

        ChavesDe(json.RootElement).ShouldBe(contrato.Request, ignoreOrder: true);
        ChavesDe(json.RootElement.GetProperty("recipient")).ShouldBe(contrato.Recipient, ignoreOrder: true);
    }

    private static string[] ChavesDe(JsonElement elemento) =>
        elemento.EnumerateObject().Select(p => p.Name).ToArray();

    private static Contrato CarregarContrato()
    {
        // Copiado para o output pelo csproj: caminho estavel em qualquer build (o [CallerFilePath]
        // vira caminho deterministico /_/... no CI e nao existe em runtime).
        var caminho = Path.Combine(AppContext.BaseDirectory, "Contracts", "hiram-events.contract.json");
        return JsonSerializer.Deserialize<Contrato>(File.ReadAllText(caminho), Web)!;
    }

    private sealed record Contrato(string[] Request, string[] Recipient);
}
