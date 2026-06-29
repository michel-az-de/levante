# Convenção de nomes, Levante

Fonte de verdade para nomenclatura. Reflete a regra mestra acordada e os defaults registrados no `CLAUDE.md`. Em conflito, este arquivo e o `CLAUDE.md` vencem.

## Regra mestra

`[ConceitoDeDomínioPT][PadrãoTécnicoEN]`. O pedaço em PT-BR nomeia o que a coisa é no negócio (linguagem ubíqua). O pedaço em EN nomeia o papel técnico (o stereotype).

Princípio: dentro do modelo de domínio (entidade, value object, evento, comportamento) tudo é PT. A casca técnica que embrulha o domínio é EN.

## Backend (C#)

| Artefato | Convenção | Exemplo |
|----------|-----------|---------|
| Entidade / Agregado | domínio PT | `Artigo`, `Cliente`, `Comentario` |
| Value Object | domínio PT | `Slug`, `Email`, `Cpf` |
| Enum (tipo e valores) | domínio PT | `StatusArtigo { Rascunho, Publicado, Arquivado }` |
| Propriedade | domínio PT | `artigo.Titulo`, `comentario.DataCriacao` |
| Comportamento (método de domínio) | verbo PT | `artigo.Publicar()`, `comentario.Aprovar()` |
| Domain Service | capacidade PT | `Precificador`, `AvaliadorDeLead` |
| Domain Event | fato PT, sem sufixo | `ArtigoPublicado`, `ComentarioAprovado` |
| Integration Event | PT + sufixo EN | `ArtigoPublicadoIntegrationEvent` |
| Command | verbo+noun PT + `Command` | `PublicarArtigoCommand`, `CriarClienteCommand` |
| Command Handler | `...CommandHandler` | `PublicarArtigoCommandHandler` |
| Query | `Obter/Listar...` PT + `Query` | `ObterArtigoPorSlugQuery` |
| Repository (interface e classe) | noun PT + `Repository` | `IArtigoRepository`, `ArtigoRepository` |
| Métodos de Repository | EN (classe técnica) | `GetBySlugAsync`, `AddAsync`, `ListAsync` |
| Consumer (mensageria) | PT + `Consumer` | `ArtigoPublicadoConsumer` |
| DTO / Request / Response | PT + EN | `ArtigoDto`, `CriarArtigoRequest`, `ArtigoResponse` |
| Validator | PT + `Validator` | `PublicarArtigoCommandValidator` |
| Endpoints (Minimal API) | noun PT + `Endpoints` | `ArtigoEndpoints` |
| Domain Exception | frase PT + `Exception` | `ArtigoNaoEncontradoException` |
| Specification | PT + `Specification` | `ArtigoPublicadoSpecification` |
| Namespace de contexto | domínio PT | `Levante.Conteudo` |
| Namespace de camada | técnico EN | `.Domain`, `.Application`, `.Infrastructure` |
| Shared kernel | técnico EN | `Levante.SharedKernel` |
| Pasta de feature | domínio PT | `Artigos/`, `Comentarios/` |
| Pasta técnica | técnico EN | `Persistence/`, `Messaging/` |

## Persistência (MongoDB)

| Item | Convenção | Exemplo |
|------|-----------|---------|
| Collection de domínio | PT minúsculo plural | `artigos`, `comentarios` |
| Campos do documento | PT camelCase | `titulo`, `dataCriacao`, `dataPublicacao` |
| Collection técnica | EN | `outbox` |

## Frontend (Next.js / TS)

| Artefato | Convenção | Exemplo |
|----------|-----------|---------|
| Tipo de domínio | PT (gerado do OpenAPI) | `Artigo`, `Comentario` |
| Componente de domínio | noun PT + papel EN | `ArtigoCard`, `ArtigoList`, `ComentarioForm` |
| Hook | `use` + noun PT | `useArtigos`, `useArtigo` |
| API client | noun PT + EN | `artigoApi` |
| Rota / página | slug PT | `/artigos/[slug]` |
| Componente genérico de UI (sem domínio) | EN | `Button`, `Modal`, `DataTable` (shadcn) |

## Sub-regras obrigatórias

1. Identificadores em código: PT sem acento e sem cedilha (`Conteudo`, `Comentario`, `Endereco`, `Publicacao`, `Codigo`). Acento e cedilha só em strings de UI, conteúdo e dados.
2. Casing: PascalCase em tipos e membros C#; camelCase em campos Mongo e em TS.
3. Auditoria no domínio em PT (`DataCriacao`, `DataAtualizacao`).
4. Mensagens e códigos de erro (Result) em PT, voltados ao usuário.
5. Commits e PRs em PT (Conventional Commits): `feat(conteudo): adiciona publicacao de artigo`.

## Defaults registrados

Pontos que o seu exemplo original não cobria e foram fixados como default (mude aqui se quiser inverter):

| Ponto | Default |
|-------|---------|
| Verbo de Command / Query | PT (`PublicarArtigoCommand`) |
| Métodos de classe técnica (Repository) | EN (`GetBySlugAsync`) |
| Contexto / módulo | PT (`Levante.Conteudo`) |
| Domain event | sem sufixo; Integration event com `IntegrationEvent` |
| Auditoria | PT (`DataCriacao`) |
| Mensagens de erro | PT |
| Commit / PR | PT |

---

## Slice de referência (contexto Conteudo, agregado Artigo)

Exemplo fim a fim de uma fatia nomeada no padrão. Serve de molde para qualquer feature nova. Código ilustrativo, focado em nomenclatura.

### Estrutura de pastas

```
contexts/Conteudo/
├─ Levante.Conteudo.Domain/
│  ├─ Artigos/
│  │  ├─ Artigo.cs                       # agregado
│  │  ├─ Slug.cs                         # value object
│  │  ├─ StatusArtigo.cs                 # enum
│  │  ├─ ArtigoPublicado.cs              # domain event
│  │  ├─ ArtigoNaoEncontradoException.cs
│  │  └─ IArtigoRepository.cs            # contrato (métodos EN)
├─ Levante.Conteudo.Application/
│  ├─ Artigos/
│  │  ├─ PublicarArtigo/
│  │  │  ├─ PublicarArtigoCommand.cs
│  │  │  ├─ PublicarArtigoCommandHandler.cs
│  │  │  └─ PublicarArtigoCommandValidator.cs
│  │  ├─ ObterArtigoPorSlug/
│  │  │  ├─ ObterArtigoPorSlugQuery.cs
│  │  │  └─ ObterArtigoPorSlugQueryHandler.cs
│  │  └─ ArtigoResponse.cs
└─ Levante.Conteudo.Infrastructure/
   ├─ Persistence/
   │  └─ ArtigoRepository.cs             # MongoDB.Driver, collection "artigos"
   └─ Messaging/
      └─ ArtigoPublicadoIntegrationEvent.cs
```

### Domain

```csharp
namespace Levante.Conteudo.Domain.Artigos;

public enum StatusArtigo { Rascunho, Publicado, Arquivado }

public sealed record Slug
{
    public string Valor { get; }
    public Slug(string valor) { /* guard clause: formato kebab-case */ Valor = valor; }
}

public sealed class Artigo
{
    public Guid Id { get; private set; }
    public string Titulo { get; private set; }
    public Slug Slug { get; private set; }
    public string Conteudo { get; private set; }
    public StatusArtigo Status { get; private set; }
    public DateTime DataCriacao { get; private set; }
    public DateTime? DataPublicacao { get; private set; }

    private readonly List<object> _eventos = [];
    public IReadOnlyList<object> Eventos => _eventos;

    public void Publicar()
    {
        if (Status == StatusArtigo.Publicado) return;
        Status = StatusArtigo.Publicado;
        DataPublicacao = DateTime.UtcNow;
        _eventos.Add(new ArtigoPublicado(Id, Slug, DataPublicacao.Value));
    }
}

public sealed record ArtigoPublicado(Guid ArtigoId, Slug Slug, DateTime DataPublicacao);

public sealed class ArtigoNaoEncontradoException(string slug)
    : Exception($"Artigo com slug '{slug}' nao encontrado.");

public interface IArtigoRepository
{
    Task<Artigo?> GetBySlugAsync(string slug, CancellationToken ct);
    Task AddAsync(Artigo artigo, CancellationToken ct);
    Task UpdateAsync(Artigo artigo, CancellationToken ct);
}
```

### Application

```csharp
namespace Levante.Conteudo.Application.Artigos.PublicarArtigo;

public sealed record PublicarArtigoCommand(Guid ArtigoId);

public sealed class PublicarArtigoCommandValidator
    : AbstractValidator<PublicarArtigoCommand>
{
    public PublicarArtigoCommandValidator() =>
        RuleFor(c => c.ArtigoId).NotEmpty();
}

public sealed class PublicarArtigoCommandHandler(IArtigoRepository repositorio)
{
    public async Task<Result> Handle(PublicarArtigoCommand comando, CancellationToken ct)
    {
        var artigo = await repositorio.GetByIdAsync(comando.ArtigoId, ct);
        if (artigo is null) return Result.Falha("Artigo nao encontrado.");

        artigo.Publicar();
        await repositorio.UpdateAsync(artigo, ct);
        // eventos de domínio -> Outbox (Integration Event) na Infrastructure
        return Result.Ok();
    }
}
```

```csharp
namespace Levante.Conteudo.Application.Artigos;

public sealed record ArtigoResponse(
    Guid Id, string Titulo, string Slug, string Conteudo, DateTime? DataPublicacao);
```

### Infrastructure

```csharp
namespace Levante.Conteudo.Infrastructure.Messaging;

public sealed record ArtigoPublicadoIntegrationEvent(
    Guid ArtigoId, string Slug, DateTime DataPublicacao);
```

Documento na collection `artigos`:

```json
{
  "_id": "5f...",
  "titulo": "Clean Architecture na pratica",
  "slug": "clean-architecture-na-pratica",
  "conteudo": "...",
  "status": "Publicado",
  "dataCriacao": "2026-01-10T12:00:00Z",
  "dataPublicacao": "2026-01-12T09:30:00Z"
}
```

### Presentation (Minimal API)

```csharp
namespace Levante.Api.Endpoints;

public static class ArtigoEndpoints
{
    public static void MapArtigoEndpoints(this IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/artigos").WithTags("Artigos");

        grupo.MapGet("/", ListarArtigosPublicados);          // ObterArtigosPublicadosQuery
        grupo.MapGet("/{slug}", ObterArtigoPorSlug);         // ObterArtigoPorSlugQuery
        grupo.MapPost("/{id:guid}/publicar", PublicarArtigo) // PublicarArtigoCommand
             .RequireAuthorization("Admin");                 // authz explícita
    }
}
```

### Frontend (Next.js / TS)

```tsx
// tipo Artigo é gerado do OpenAPI, não escrito à mão

export function ArtigoCard({ artigo }: { artigo: Artigo }) { /* ... */ }

export function useArtigos() { /* fetch via artigoApi */ }

// rota: src/web/app/artigos/[slug]/page.tsx  ->  /artigos/clean-architecture-na-pratica
```

Resumo do padrão no slice: domínio em PT (`Artigo`, `Slug`, `StatusArtigo`, `Publicar`, `ArtigoPublicado`), casca técnica em EN (`Repository`, `Command`, `Handler`, `IntegrationEvent`, `GetBySlugAsync`), collection e campos em PT camelCase, front com componente `ArtigoCard` e rota com slug PT.
