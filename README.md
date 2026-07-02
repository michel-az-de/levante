# Levante

Plataforma pessoal e portfólio técnico de Felipe Michel de Azevedo, arquiteto de soluções e desenvolvedor sênior .NET / full stack. Blog técnico, publicações, vitrine de projetos e hub de identidade.

> Da pedra bruta à pedra polida.

[![CI](https://github.com/michel-az-de/levante/actions/workflows/ci.yml/badge.svg)](https://github.com/michel-az-de/levante/actions/workflows/ci.yml)

## Stack

| Camada | Tecnologia |
|--------|-----------|
| Backend | .NET (LTS) · Minimal API · Clean Architecture · DDD · SOLID · CQRS-lite |
| Banco | MongoDB Atlas |
| Frontend | Next.js (SSR/SSG) · React · TypeScript · Tailwind + shadcn/ui |
| Mensageria | Outbox → RabbitMQ (entrega via Hiram) |
| Infra | Azure · GitHub Actions |

## Arquitetura

Monólito modular. Cada bounded context (`Conteudo`, `Engajamento`, `Audiencia`, `Analytics`, `Identity`, `Documents`, `Portfolio`) é um módulo com Clean Architecture própria. Notificações são responsabilidade de outro projeto (Hiram); aqui os eventos são publicados via Outbox.

```
Navegador → Next.js (SSR) → [OpenAPI] → .NET Minimal API → Domain → MongoDB
                                              └→ Outbox → RabbitMQ → Hiram → email/push/WhatsApp
```

## Documentação

| Arquivo | Conteúdo |
|---------|----------|
| `docs/blueprint.md` | Visão e arquitetura E2E |
| `docs/mapa-tecnico.md` | Todas as decisões técnicas com status |
| `docs/mapa-tecnico.html` | Versão visual do mapa (abrir no navegador) |
| `docs/convencao-de-nomes.md` | Convenção de nomes + slice de referência |
| `CLAUDE.md` | Guia operacional para desenvolvimento assistido |

## Qualidade e segurança

Esteira com gates em sequência: `rough-cut → dress → polish → raise`. Arch tests, analyzers, CodeQL, gitleaks, NuGet audit e testes de isolamento multi-tenant. Nada vai a produção sem passar todos os gates.

## Desenvolvimento local

Pré-requisitos: **.NET 10 SDK** (fixado em `global.json`), **Node 20+** (Next.js 16) e **Docker** (testes de integração com Testcontainers).

```bash
# Backend
dotnet build src/api/Levante.sln
dotnet test  src/api/Levante.sln --filter "Category!=Integration"   # unit (sem Docker)
dotnet test  src/api/Levante.sln --filter "Category=Integration"    # integração (Docker)

# Segredos via user-secrets (nunca no repo): Mongo, JWT, admin de dev e HMAC de engajamento
dotnet user-secrets --project src/api/host/Levante.Api set "Mongo:ConnectionString" "<sua-uri-atlas>"
dotnet user-secrets --project src/api/host/Levante.Api set "Jwt:SecretKey" "<>=32-chars>"
dotnet user-secrets --project src/api/host/Levante.Api set "Admin:Email" "voce@exemplo.com"
dotnet user-secrets --project src/api/host/Levante.Api set "Admin:SenhaInicial" "<senha-forte>"
# Engajamento: segredo do HMAC que pseudonimiza IP+User-Agent na dedup de reacao (LGPD: IP nunca guardado cru)
dotnet user-secrets --project src/api/host/Levante.Api set "Engajamento:OrigemHashSecret" "<>=32-chars>"
dotnet run --project src/api/host/Levante.Api      # /health, /artigos, /auth/*, /admin/artigos, /openapi/v1.json
# Admin (Next.js): /admin/login -> /admin -> /admin/artigos (CRUD + publicar/arquivar, JWT bearer).
# Endpoints de escrita exigem token: POST/PUT /artigos, POST /artigos/{id}/publicar|arquivar.
# Em prod, semeie o admin por passo seguro unico.

# Contrato OpenAPI -> tipos TS
dotnet run --project src/api/host/Levante.Api -- --emit-openapi "$PWD/src/web/openapi/levante.json"
cd src/web && npm ci && npm run gen:api && npm run dev   # http://localhost:3000/artigos

# SEO: URL base do site (canonical, sitemap, RSS, OG) via env (GAP-A: domínio em aberto)
SITE_URL=https://seu-dominio npm run build
# Rotas de SEO: /sitemap.xml  /robots.txt  /feed.xml  e /artigos/[slug] (SSR + JSON-LD)

# Hooks de pré-commit (format + gitleaks)
dotnet tool restore && dotnet husky install            # gitleaks deve estar no PATH
```

## Status

Em construção. Roadmap vigente em [`docs/roadmap.md`](docs/roadmap.md): Fase A (fundações — testes de front, higiene de CI, contrato de erro, BFF do admin) → Engajamento → Outbox+Audiência (Hiram) → lançamento (Azure Container Apps, PT-only) → portfólio + leads = MVP concluído.

Fase B em andamento — B1 (reações): contexto novo `Engajamento` com **reações anônimas** (curtir/amei/relevante) por artigo. Unicidade por **cookie de visitante** (`lev_vid`, httpOnly first-party, id opaco) + hash de IP como sinal anti-abuso (LGPD: IP nunca guardado cru). A escrita pública passa por um **BFF público** no Next (`/api/publico/*`), mantendo a invariante de que o browser nunca fala com a API direto.

Entregue até aqui: Fatia 2c-ii (taxonomia: **Categoria** como agregado próprio e **Tags** embutidas, browse `/categoria/[slug]`), 2c-i (meta SEO editável por artigo), 2b (CRUD de artigos pelo `/admin`, markdown, publicar/arquivar), 2a (autenticação do admin, JWT bearer), 1 (conteúdo público + SEO base) e 0 (walking skeleton). (Página de tag — `/tag/[slug]` — fica para depois.)

## Licença

O código-fonte é licenciado sob Apache-2.0 (ver `LICENSE`).

O nome "Levante", a identidade visual, o design system e todo o conteúdo editorial (artigos, publicações, textos) não estão cobertos pela licença e são de uso reservado. Reutilize os padrões de arquitetura à vontade; a marca e o conteúdo, não.
