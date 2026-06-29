using System.Text;
using Levante.Api.Endpoints;
using Levante.Api.Seguranca;
using Levante.Conteudo.Application.Artigos.ListarArtigosPublicados;
using Levante.Conteudo.Application.Artigos.ObterArtigoPorSlug;
using Levante.Conteudo.Infrastructure;
using Levante.Identity.Application.Autenticacao;
using Levante.Identity.Infrastructure;
using Levante.Identity.Infrastructure.Seguranca;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
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
// A chave/issuer/audience vem de JwtOptions resolvido LAZY (config final). Isso e
// essencial nos testes: o WebApplicationFactory injeta config DEPOIS do builder,
// entao ler eager daria divergencia entre gerar e validar o token. ValidateOnStart
// de JwtOptions exige o segredo no boot real; o fallback so cobre o modo emit.
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();
builder.Services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
    .Configure<IOptions<JwtOptions>>((bearer, jwt) =>
    {
        var opcoes = jwt.Value;
        var secret = string.IsNullOrEmpty(opcoes.SecretKey)
            ? "emit-only-fallback-key-defina-Jwt-SecretKey-em-user-secrets"
            : opcoes.SecretKey;

        bearer.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
            ValidateIssuer = true,
            ValidIssuer = opcoes.Issuer,
            ValidateAudience = true,
            ValidAudience = opcoes.Audience,
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

// CORS: o admin (Next.js) chama a API direto do browser com JWT bearer.
// Origem(ns) via config (default localhost:3000 em dev). Sem AllowCredentials
// (token vai no header Authorization, nao em cookie).
var corsOrigens = builder.Configuration.GetSection("Cors:Origens").Get<string[]>()
    ?? ["http://localhost:3000"];
builder.Services.AddCors(options => options.AddDefaultPolicy(policy =>
    policy.WithOrigins(corsOrigens).AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseSecurityHeaders();
app.UseCors();
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
