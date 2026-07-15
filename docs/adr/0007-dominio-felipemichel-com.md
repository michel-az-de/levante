# ADR 0007 — Domínio do site: felipemichel.com (apex canônico)

Status: **Aceito** · jul/2026 · Fecha o **GAP-A** (domínio), registrado como "pendente/indefinido" em `docs/mapa-tecnico.md`, `docs/roadmap.md`, `docs/plano-mvp-producao.md`, `docs/lancamento-runbook.md`, `README.md` e `CLAUDE.md`.

## Contexto

O GAP-A (qual domínio serviria o site) ficou deliberadamente em aberto durante toda a construção: o código nasceu *domain-agnostic* (canonical, `sitemap`, RSS, OG, JSON-LD e `robots` leem `SITE_URL`/`SITE_HOST` via env; nada hardcoded — ver `src/web/src/lib/site.ts`, `sitemap.ts`, `feed.xml/route.ts`, `robots.ts`, `middleware.ts`). A decisão foi carimbada como um marco **dentro** do cutover (D0), não como pré-requisito da fase de lançamento.

Com o produto *code-complete* e a Fase D (cutover) em curso, o domínio foi **comprado**: `felipemichel.com`. O site é a plataforma pessoal e portfólio do Felipe (blog técnico, publicações, vitrine de projetos), alimentada pelo Levante; a landing do produto vive no mesmo app em `/levante` (route group `(produto)`). Um único host serve tudo.

Fixar o domínio destrava os passos finais do cutover (DNS/TLS, canonical/OG "assados" na URL real, indexação, newsletter) e não é reversível de graça — SEO/`canonical`/backlinks se apoiam num host estável — por isso vira ADR (R10/DoD: reabrir ou registrar decisão exige ADR).

## Decisão

1. **Domínio: `felipemichel.com`.** Substitui o host interino `sslip.io` usado enquanto o GAP-A esteve aberto.
2. **Apex é o canônico.** `SITE_HOST=felipemichel.com`, `SITE_URL=https://felipemichel.com`. Tudo que "assa" a URL (canonical, OG, JSON-LD, RSS, sitemap) usa o apex, sem `www` e sem barra final (`site.url` já normaliza a trailing slash).
3. **`www.felipemichel.com` → 301 para o apex.** Redirect permanente na borda (Caddy), não um segundo host indexável. Consolida a autoridade de SEO num único host e evita conteúdo duplicado. **Requer** um bloco `www.{$SITE_HOST}` no `Caddyfile` da stack (repo Hiram) — hoje o Caddy só serve `{$SITE_HOST}` (ver Consequências).
4. **Nada de hardcode.** A decisão não introduz o domínio no código de runtime: continua tudo via `SITE_URL`/`SITE_HOST`/env. As menções ao domínio ficam em docs, exemplos (`*.env.example`, comentários) e no `.env` da VM (não versionado).
5. **Indexação e newsletter permanecem atrás de flags explícitos.** `SITE_INDEXABLE` e `NEWSLETTER_ENABLED` só ligam no cutover D0, depois de DNS/TLS válidos e (para a newsletter) do domínio de envio Resend verificado. A compra do domínio não liga nada sozinha.

## Consequências

- `CLAUDE.md`, `docs/lancamento-runbook.md`, `docs/mapa-tecnico.md`, `docs/roadmap.md`, `docs/plano-mvp-producao.md` e `README.md` deixam de descrever o GAP-A como "pendente/indefinido/em aberto" — passam a apontar para este ADR e para `felipemichel.com`.
- Nasce a folha de cutover concreta `docs/cutover-felipemichel-com.md` (DNS apex+www+Resend, valores de `.env` da stack, secrets/variables do CD, sequência D0), referenciando o runbook sem duplicá-lo.
- **Pendências no repo Hiram (PR à parte, pré-requisito do cutover)**, registradas na folha:
  1. **`www` → apex:** adicionar `www.{$SITE_HOST} { redir https://{$SITE_HOST}{uri} permanent }` ao `Caddyfile` da stack. Sem isso, `www.felipemichel.com` não resolve (nem redireciona).
  2. **Flags no compose:** o serviço `levante-web` do `docker-compose.yml` só passa `NODE_ENV/PORT/API_BASE_URL/SITE_URL`. Sem plumbing de `SITE_INDEXABLE` e `NEWSLETTER_ENABLED` (do `.env` para o container), os passos D0 de habilitar indexação e ativar a newsletter **não têm efeito**.
- Nenhuma mudança de contrato: o OpenAPI (`levante.json`) não muda; nenhum campo novo em agregado.

## Alternativas consideradas

- **`www` como canônico (apex → www):** rejeitada. Para um site pessoal, o apex é mais limpo e memorável; o `www` vira o redirect. Simétrico em esforço, então venceu a preferência estética/clareza.
- **Manter só o apex, sem tratar `www`:** rejeitada. Quem digitar `www.felipemichel.com` tomaria erro de conexão; o custo do redirect é um bloco de 3 linhas no Caddy.
- **Subdomínio (ex.: `blog.` / `www.` como principal):** rejeitada. O site é a home pessoal do Felipe; o apex é o lugar natural. O produto Levante fica em `/levante`, não em subdomínio.
- **Adiar a decisão (seguir no `sslip.io`):** rejeitada. O domínio já foi comprado e o interino não indexa (flag `SITE_INDEXABLE` off) — segurar o apex só atrasa o go-live sem ganho.
