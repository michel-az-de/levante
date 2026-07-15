# Roadmap, Levante

Sequenciamento vigente das fatias, derivado do blueprint (§14) e de uma auditoria completa do repositório (jul/2026). Substitui a numeração original das fatias 3+ do blueprint; o blueprint permanece como visão, este arquivo como ordem de execução.

## Definição de "concluído" (v1 / MVP lançável)

Site no ar te vendendo: conteúdo público + admin (entregues) + engajamento + audiência com notificações via Hiram + **portfólio + leads**, em produção no domínio final. Todo o resto é evolução contínua, sem compromisso de escopo.

Decisões já tomadas: hospedagem = **VM conjunta com o Hiram via Docker Compose** (GAP-J resolvido, [ADR 0003](adr/0003-hospedagem-vm-conjunta-hiram.md)), idioma = **chrome bilíngue PT/EN, conteúdo de artigo continua PT-only** (GAP-H reaberto, [ADR 0005](adr/0005-idioma-chrome-bilingue.md)), **contrato Hiram = HTTP `POST /v1/events` (GAP-I resolvido, [ADR 0002](adr/0002-emissao-hiram-http.md))**, **domínio = `felipemichel.com`** (apex canônico, `www`→301; GAP-A resolvido, [ADR 0007](adr/0007-dominio-felipemichel-com.md)) — o cutover D0 (dentro de D3) apenas aponta DNS/`SITE_URL`, sem bloquear o trabalho técnico anterior.

O plano operacional da reta final (D+E), com as decisões de produção e o checklist de go-live, está em [plano-mvp-producao.md](plano-mvp-producao.md).

## Fase A — Fundações rápidas (1 PR por fatia)

Dívidas baratas que apodrecem a cada contexto novo. Fazer antes de multiplicar o padrão.

| # | Fatia | Tamanho | Critério de pronto |
|---|-------|---------|--------------------|
| A1 | Higiene de CI: `[Trait(Category)]` nos unit tests, split unit/integração no polish, cobertura na Application (ou ADR), NuGet audit explícito, `MongoDbBuilder(image)` | P | **Entregue.** `ci.yml` com split `Category!=Integration`/`=Integration` e cobertura como gate (`Threshold=80`) por assembly |
| A2 | Harness de testes do frontend (Vitest + Testing Library) + job no polish | P/M | **Entregue.** `npm test` no job `polish`; 13 arquivos `*.test.ts(x)` em `src/web/src` |
| A3 | Contrato de erro Result→HTTP consistente (400/404/409/500, ProblemDetails); validação de `MetaSeo` no validator; rehidratação tolerante | P | **Entregue.** `ResultadoHttpTests` asserta status por tipo de `Error` + ProblemDetails |
| A4 | Consolidação de MongoOptions em módulo compartilhado (TODO existente) | P | **Entregue.** `MongoOptions` único no `SharedKernel.Infrastructure`; arch tests verdes |
| A5 | Admin JWT: localStorage → cookie httpOnly via BFF no Next + CSRF | M | **Entregue.** Cookie httpOnly em `sessao/route.ts`; proxy admin 403 em origem cruzada |

## Fase B — Engajamento (contexto novo)

| # | Fatia | Tamanho | Notas |
|---|-------|---------|-------|
| B1 | Reações/curtidas anônimas (rate limit + honeypot) | P/M | **Entregue.** Contexto `Engajamento` (`Reacao`, endpoints com rate limit, `ReacoesArtigo.tsx` + teste) |
| B2 | Comentários com fila de moderação + anti-spam | G | **Entregue.** `Comentario` (Pendente→Aprovado/Rejeitado), honeypot, `/admin/comentarios`; `ComentarioCriado` no Outbox |

## Fase C — Outbox + Audiência (a arquitetura de eventos vira real)

| # | Fatia | Tamanho | Notas |
|---|-------|---------|-------|
| C0 | Spike GAP-F (mediator/Wolverine) + contrato Hiram (GAP-I) → ADR | P | Decisão do Felipe antes de C1; define envelope do IntegrationEvent |
| C1 | Outbox transacional + relay (reconciliação/polling) + publisher RabbitMQ | G | **Entregue.** Evento na mesma transação do agregado; at-least-once (dedup por `eventId`); Testcontainers (Mongo + Rabbit). Polling em vez de Change Streams (ADR 0001) |
| C2 | Audiencia: newsletter double opt-in via Hiram | G | **Entregue.** Agregado `Assinante`; consentimento LGPD com timestamp; token opaco; site nunca chama provedor de e-mail; não vaza existência de e-mail |
| C3 | Notificação de comentário pendente via Hiram | P | **Entregue (código).** `ComentarioCriado` → `comentario_pendente` no `MapeadorDeEmissao`, emitido pelo relay HTTP ([ADR 0002](adr/0002-emissao-hiram-http.md)). Entrega **fim-a-fim** depende do Hiram em produção (dependência operacional) |

## Fase D — Lançamento

| # | Fatia | Tamanho | Notas |
|---|-------|---------|-------|
| D0 | **Cutover do domínio (`felipemichel.com`)** | — | GAP-A **resolvido** (apex; [ADR 0007](adr/0007-dominio-felipemichel-com.md)). Marco dentro de D3: DNS/TLS → `SITE_URL`/canonical → indexação → newsletter → Search Console. Valores em [cutover-felipemichel-com.md](cutover-felipemichel-com.md) |
| D1 | Observabilidade mínima: logs JSON + OpenTelemetry (OTLP) → `otel-lgtm` na VM (Tempo/Loki/Prometheus) | M | No ar junto com o primeiro deploy, não depois |
| D2 | Vitrine de identidade (home/`/sobre`) + wa.me + política de privacidade | P/M | LGPD base: comentários e newsletter já coletam dados pessoais |
| D3 | Deploy na VM conjunta (Compose): imagens `levante-api`+`levante-web` no GHCR, Mongo Atlas (privilégio mínimo), CORS/CSP prod, DNS/TLS via Caddy, CD escopado pós-`raise` com environment protection, Search Console | M/G | Pronto: merge na main → produção ([ADR 0003](adr/0003-hospedagem-vm-conjunta-hiram.md)); securityheaders.com nota A |

## Fase E — Completar o MVP (em produção)

| # | Fatia | Tamanho | Notas |
|---|-------|---------|-------|
| E1 | Leads: form + mini-CRM + evento `NovoLead` via Hiram | M | Contexto Audiencia; UTM/origem no agregado |
| E2 | Portfolio: agregado `Projeto` + `/projetos` (admin + SSG + JSON-LD + sitemap) | M | Molde de Conteudo |
| E3 | Integração GitHub (GraphQL: repos, contribuições) com cache TTL/ISR | M | Degradação graciosa; webhook de invalidação = TODO |

**→ MVP concluído.**

## Evolução contínua (pós-MVP, sem compromisso)

`/tag/[slug]` (quick win, cabe em qualquer folga) · MFA TOTP · Analytics first-party + banner de consentimento (banner ANTES de rastrear) · dashboard admin · página `/arquitetura` (meta-portfólio) · Documents (bloqueado por GAP-B/GAP-C; decidir só com demanda real) · design tokens / passada de Core Web Vitals. tradução do corpo de artigo (i18n de conteúdo — distinto do chrome bilíngue PT/EN já reaberto, GAP-H/ADR 0005).

## Dívidas conhecidas fora das fatias (oportunistas)

`.env.example` do web · revalidate ISR centralizado · int32 gerando `unknown` nos tipos TS do contrato · baseline de acessibilidade (aria/alt) nas fatias de front que tocarem os componentes.
