# Roadmap, Levante

Sequenciamento vigente das fatias, derivado do blueprint (§14) e de uma auditoria completa do repositório (jul/2026). Substitui a numeração original das fatias 3+ do blueprint; o blueprint permanece como visão, este arquivo como ordem de execução.

## Definição de "concluído" (v1 / MVP lançável)

Site no ar te vendendo: conteúdo público + admin (entregues) + engajamento + audiência com notificações via Hiram + **portfólio + leads**, em produção no domínio final. Todo o resto é evolução contínua, sem compromisso de escopo.

Decisões já tomadas: hospedagem = Azure Container Apps (GAP-J), idioma = PT-only (GAP-H), contrato Hiram definível no spike do Outbox (GAP-I). **Pendente: domínio (GAP-A) — única decisão externa que bloqueia o lançamento.**

## Fase A — Fundações rápidas (1 PR por fatia)

Dívidas baratas que apodrecem a cada contexto novo. Fazer antes de multiplicar o padrão.

| # | Fatia | Tamanho | Critério de pronto |
|---|-------|---------|--------------------|
| A1 | Higiene de CI: `[Trait(Category)]` nos unit tests, split unit/integração no polish, cobertura na Application (ou ADR), NuGet audit explícito, `MongoDbBuilder(image)` | P | `--filter "Category!=Integration"` filtra de verdade; CI com steps separados |
| A2 | Harness de testes do frontend (Vitest + Testing Library) + job no polish | P/M | `npm test` existe e quebra o CI; regra "fatia de UI nasce com teste" |
| A3 | Contrato de erro Result→HTTP consistente (400/404/409/500, ProblemDetails); validação de `MetaSeo` no validator; rehidratação tolerante | P | Teste de integração asserta status code por classe de erro |
| A4 | Consolidação de MongoOptions em módulo compartilhado (TODO existente) | P | Uma única definição da seção `Mongo`; arch tests verdes |
| A5 | Admin JWT: localStorage → cookie httpOnly via BFF no Next + CSRF | M | Nenhum token em localStorage; obrigatório antes de comentários |

## Fase B — Engajamento (contexto novo)

| # | Fatia | Tamanho | Notas |
|---|-------|---------|-------|
| B1 | Reações/curtidas anônimas (rate limit + honeypot) | P/M | Primeiro contexto novo; replica o molde de Conteudo já com o contrato de erro da A3 |
| B2 | Comentários com fila de moderação + anti-spam | G | `StatusComentario { Pendente, Aprovado, Rejeitado }`; depende de A5; eventos de domínio prontos para o Outbox |

## Fase C — Outbox + Audiência (a arquitetura de eventos vira real)

| # | Fatia | Tamanho | Notas |
|---|-------|---------|-------|
| C0 | Spike GAP-F (mediator/Wolverine) + contrato Hiram (GAP-I) → ADR | P | Decisão do Felipe antes de C1; define envelope do IntegrationEvent |
| C1 | Outbox transacional + Change Streams relay + publisher RabbitMQ | G | Evento na mesma transação do agregado; resume token; idempotência; Testcontainers (Mongo RS + Rabbit) |
| C2 | Audiencia: newsletter double opt-in via Hiram | G | Consentimento LGPD registrado; site nunca chama provedor de e-mail |
| C3 | Notificação de comentário pendente via Hiram | P | Retrofit barato de B2 |

## Fase D — Lançamento

| # | Fatia | Tamanho | Notas |
|---|-------|---------|-------|
| D0 | **Decisão GAP-A (domínio)** | — | Do Felipe; sem ela a fase não inicia |
| D1 | Observabilidade mínima: Serilog JSON + OpenTelemetry → App Insights | M | No ar junto com o primeiro deploy, não depois |
| D2 | Vitrine de identidade (home/`/sobre`) + wa.me + política de privacidade | P/M | LGPD base: comentários e newsletter já coletam dados pessoais |
| D3 | IaC (Bicep) + deploy Container Apps: API + web, Key Vault, CORS/CSP prod, DNS/TLS, deploy pós-`raise` com environment protection, Search Console | M/G | Pronto: merge na main → produção; securityheaders.com nota A |

## Fase E — Completar o MVP (em produção)

| # | Fatia | Tamanho | Notas |
|---|-------|---------|-------|
| E1 | Leads: form + mini-CRM + evento `NovoLead` via Hiram | M | Contexto Audiencia; UTM/origem no agregado |
| E2 | Portfolio: agregado `Projeto` + `/projetos` (admin + SSG + JSON-LD + sitemap) | M | Molde de Conteudo |
| E3 | Integração GitHub (GraphQL: repos, contribuições) com cache TTL/ISR | M | Degradação graciosa; webhook de invalidação = TODO |

**→ MVP concluído.**

## Evolução contínua (pós-MVP, sem compromisso)

`/tag/[slug]` (quick win, cabe em qualquer folga) · MFA TOTP · Analytics first-party + banner de consentimento (banner ANTES de rastrear) · dashboard admin · página `/arquitetura` (meta-portfólio) · Documents (bloqueado por GAP-B/GAP-C; decidir só com demanda real) · design tokens / passada de Core Web Vitals. i18n descartado enquanto GAP-H = PT-only.

## Dívidas conhecidas fora das fatias (oportunistas)

`.env.example` do web · revalidate ISR centralizado · int32 gerando `unknown` nos tipos TS do contrato · baseline de acessibilidade (aria/alt) nas fatias de front que tocarem os componentes.
