using System.Text;
using FluentValidation;
using Levante.Api.Endpoints;
using Levante.Api.Seguranca;
using Levante.Audiencia.Application.Assinantes.CancelarAssinatura;
using Levante.Audiencia.Application.Assinantes.ConfirmarAssinatura;
using Levante.Audiencia.Application.Assinantes.SolicitarAssinatura;
using Levante.Audiencia.Infrastructure;
using Levante.Conteudo.Application.Artigos.ArquivarArtigo;
using Levante.Conteudo.Application.Artigos.CriarArtigo;
using Levante.Conteudo.Application.Artigos.EditarArtigo;
using Levante.Conteudo.Application.Artigos.ListarArtigosPublicados;
using Levante.Conteudo.Application.Artigos.ListarArtigosPublicadosPorCategoria;
using Levante.Conteudo.Application.Artigos.ListarTodosArtigos;
using Levante.Conteudo.Application.Artigos.ObterArtigoPorSlug;
using Levante.Conteudo.Application.Artigos.PublicarArtigo;
using Levante.Conteudo.Application.Categorias.CriarCategoria;
using Levante.Conteudo.Application.Categorias.EditarCategoria;
using Levante.Conteudo.Application.Categorias.ListarCategorias;
using Levante.Conteudo.Infrastructure;
using Levante.Engajamento.Application.Comentarios.AprovarComentario;
using Levante.Engajamento.Application.Comentarios.CriarComentario;
using Levante.Engajamento.Application.Comentarios.ListarComentariosAprovados;
using Levante.Engajamento.Application.Comentarios.ListarComentariosPendentes;
using Levante.Engajamento.Application.Comentarios.RejeitarComentario;
using Levante.Engajamento.Application.Reacoes.ObterReacoesDoArtigo;
using Levante.Engajamento.Application.Reacoes.RegistrarReacao;
using Levante.Engajamento.Application.Reacoes.RemoverReacao;
using Levante.Engajamento.Infrastructure;
using Levante.Identity.Application.Autenticacao;
using Levante.Identity.Infrastructure;
using Levante.Identity.Infrastructure.Seguranca;
using Levante.SharedKernel.Infrastructure.Outbox;
using Levante.SharedKernel.Infrastructure.Telemetry;
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
builder.Services.AddEngajamentoInfrastructure(builder.Configuration, registrarServicosDeBoot: !modoEmitOpenApi);
builder.Services.AddIdentityInfrastructure(builder.Configuration, registrarServicosDeBoot: !modoEmitOpenApi);
builder.Services.AddAudienciaInfrastructure(builder.Configuration, registrarServicosDeBoot: !modoEmitOpenApi);

// Relay do Outbox -> HTTP Hiram (so quando Outbox:RelayHabilitado e nao e modo emit).
builder.Services.AddLevanteOutboxRelay(builder.Configuration, ligarNoBoot: !modoEmitOpenApi);

// OpenTelemetry (traces/metricas/logs -> OTLP). Fora do modo emit (nao instrumenta o emit).
if (!modoEmitOpenApi)
{
    builder.Services.AddLevanteTelemetry(builder.Configuration);
}

if (modoEmitOpenApi)
{
    builder.WebHost.UseUrls(Levante.Api.OpenApiExport.UrlEfemera);
}

// Handlers CQRS-lite chamados direto (sem mediator por ora, GAP-F).
builder.Services.AddScoped<ListarArtigosPublicadosQueryHandler>();
builder.Services.AddScoped<ObterArtigoPorSlugQueryHandler>();
builder.Services.AddScoped<ListarTodosArtigosQueryHandler>();
builder.Services.AddScoped<CriarArtigoCommandHandler>();
builder.Services.AddScoped<EditarArtigoCommandHandler>();
builder.Services.AddScoped<PublicarArtigoCommandHandler>();
builder.Services.AddScoped<ArquivarArtigoCommandHandler>();
builder.Services.AddScoped<ListarArtigosPublicadosPorCategoriaQueryHandler>();
builder.Services.AddScoped<ListarCategoriasQueryHandler>();
builder.Services.AddScoped<CriarCategoriaCommandHandler>();
builder.Services.AddScoped<EditarCategoriaCommandHandler>();
builder.Services.AddScoped<AutenticarCommandHandler>();

// Handlers do contexto Engajamento (reacoes).
builder.Services.AddScoped<ObterReacoesDoArtigoQueryHandler>();
builder.Services.AddScoped<RegistrarReacaoCommandHandler>();
builder.Services.AddScoped<RemoverReacaoCommandHandler>();

// Handlers do contexto Engajamento (comentarios).
builder.Services.AddScoped<ListarComentariosAprovadosQueryHandler>();
builder.Services.AddScoped<ListarComentariosPendentesQueryHandler>();
builder.Services.AddScoped<CriarComentarioCommandHandler>();
builder.Services.AddScoped<AprovarComentarioCommandHandler>();
builder.Services.AddScoped<RejeitarComentarioCommandHandler>();

// Handlers do contexto Audiencia (newsletter, double opt-in).
builder.Services.AddScoped<SolicitarAssinaturaCommandHandler>();
builder.Services.AddScoped<ConfirmarAssinaturaCommandHandler>();
builder.Services.AddScoped<CancelarAssinaturaCommandHandler>();

// Validators FluentValidation (IValidator<T> por comando), um por contexto.
builder.Services.AddValidatorsFromAssemblyContaining<CriarArtigoCommandValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<RegistrarReacaoCommandValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<SolicitarAssinaturaCommandValidator>();

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
app.MapArtigoAdminEndpoints();
app.MapCategoriaEndpoints();
app.MapReacaoEndpoints();
app.MapComentarioEndpoints();
app.MapNewsletterEndpoints();
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
