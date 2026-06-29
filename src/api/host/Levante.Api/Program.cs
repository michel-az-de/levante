using System.Text;
using Levante.Api.Endpoints;
using Levante.Api.Seguranca;
using Levante.Conteudo.Application.Artigos.ListarArtigosPublicados;
using Levante.Conteudo.Application.Artigos.ObterArtigoPorSlug;
using Levante.Conteudo.Infrastructure;
using Levante.Identity.Application.Autenticacao;
using Levante.Identity.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

var modoEmitOpenApi = args.Contains(Levante.Api.OpenApiExport.Argumento);

// Infraestrutura do contexto Conteudo: Mongo, repositorio, healthcheck,
// inicializacao (indices + seed) e self-check de privilegio minimo no boot.
// Em modo emit do contrato, nao registra os servicos de boot (nao toca o Mongo).
builder.Services.AddConteudoInfrastructure(builder.Configuration, registrarServicosDeBoot: !modoEmitOpenApi);
builder.Services.AddIdentityInfrastructure(builder.Configuration, registrarServicosDeBoot: !modoEmitOpenApi);

if (modoEmitOpenApi)
{
    builder.WebHost.UseUrls(Levante.Api.OpenApiExport.UrlEfemera);
}

// Handlers CQRS-lite chamados direto (sem mediator por ora, GAP-F).
builder.Services.AddScoped<ListarArtigosPublicadosQueryHandler>();
builder.Services.AddScoped<ObterArtigoPorSlugQueryHandler>();
builder.Services.AddScoped<AutenticarCommandHandler>();

// Contrato OpenAPI (consumido pelo Next.js via tipos gerados).
builder.Services.AddOpenApi();

// Autenticacao JWT bearer: da um esquema real a FallbackPolicy (deny-by-default).
// SecretKey vem de user-secrets/env (ValidateOnStart exige no boot real); o fallback
// abaixo so e usado no modo emit (sem requests) para nao exigir segredo no build.
var jwtSecret = builder.Configuration["Jwt:SecretKey"];
if (string.IsNullOrEmpty(jwtSecret))
{
    jwtSecret = "emit-only-fallback-key-defina-Jwt-SecretKey-em-user-secrets";
}

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "levante",
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"] ?? "levante",
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30),
        };
    });

// Autorizacao explicita: nega por padrao. Todo endpoint declara AllowAnonymous
// ou RequireAuthorization.
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
app.UseAuthentication();
app.UseAuthorization();

app.MapOpenApi().AllowAnonymous();
app.MapHealthEndpoints();
app.MapArtigoEndpoints();
app.MapAuthEndpoints();

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
