using Levante.Api.Endpoints;
using Levante.Api.Seguranca;
using Levante.Conteudo.Application.Artigos.ListarArtigosPublicados;
using Levante.Conteudo.Infrastructure;
using Microsoft.AspNetCore.Authorization;

var builder = WebApplication.CreateBuilder(args);

var modoEmitOpenApi = args.Contains(Levante.Api.OpenApiExport.Argumento);

// Infraestrutura do contexto Conteudo: Mongo, repositorio, healthcheck,
// inicializacao (indices + seed) e self-check de privilegio minimo no boot.
// Em modo emit do contrato, nao registra os servicos de boot (nao toca o Mongo).
builder.Services.AddConteudoInfrastructure(builder.Configuration, registrarServicosDeBoot: !modoEmitOpenApi);

if (modoEmitOpenApi)
{
    builder.WebHost.UseUrls(Levante.Api.OpenApiExport.UrlEfemera);
}

// Handlers CQRS-lite chamados direto (sem mediator por ora, GAP-F).
builder.Services.AddScoped<ListarArtigosPublicadosQueryHandler>();

// Contrato OpenAPI (consumido pelo Next.js via tipos gerados).
builder.Services.AddOpenApi();

// Autorizacao explicita: nega por padrao. Todo endpoint declara AllowAnonymous
// ou RequireAuthorization. Esquemas de autenticacao chegam com Identity (Fatia 2).
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

builder.Services.AddLevanteRateLimiting();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseSecurityHeaders();
app.UseRateLimiter();
app.UseAuthorization();

app.MapOpenApi().AllowAnonymous();
app.MapHealthEndpoints();
app.MapArtigoEndpoints();

// Modo de emissao do contrato OpenAPI (porta efemera, sem tocar o Mongo).
if (modoEmitOpenApi)
{
    var indice = Array.IndexOf(args, Levante.Api.OpenApiExport.Argumento);
    var caminho = indice >= 0 && indice + 1 < args.Length ? args[indice + 1] : "openapi.json";
    await Levante.Api.OpenApiExport.EmitAndExitAsync(app, caminho);
    return;
}

await app.RunAsync();

/// <summary>Ponto de entrada exposto como partial para os testes de integracao.</summary>
[System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Major Code Smell",
    "S1118:Utility classes should not have public constructors",
    Justification = "Program e o ponto de entrada (top-level statements); usado como marcador por WebApplicationFactory.")]
public partial class Program;
