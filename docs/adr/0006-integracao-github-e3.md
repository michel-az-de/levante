# ADR 0006 — Integração GitHub (fatia E3) mora no Next.js, não no .NET

Status: **Aceito** · jul/2026 · Formaliza a fatia E3 do `docs/roadmap.md`, ainda sem código

## Contexto

O `docs/roadmap.md` já previa a fatia **E3 — Integração GitHub** ("repos/contribuições via GraphQL, cache TTL/ISR"), sem decisão registrada de onde ela mora. O `docs/blueprint.md` §10 (documento de visão, anterior ao pivot para Next.js) cogitava um cliente GitHub dentro de `Portfolio.Infrastructure` (.NET) — mas essa visão ainda falava em Blazor, já abandonado na prática. O termo "ISR" que o próprio roadmap usa é vocabulário Next.js, não .NET — um indício textual de que a intenção real já pendia para o front.

Ao planejar o redesign do site público (bento "Código aberto" no site pessoal + card de repositório/issues/commits na landing do produto Levante), o Felipe decidiu implementar essa fatia agora, com dados ao vivo (não um placeholder estático). Esta ADR fecha a decisão de arquitetura antes do código (fatia separada).

## Decisão

1. **A integração mora inteiramente em `src/web`** (`lib/github/*`, Server Components/Route Handlers internos), **não no `.NET`**. É decoração de apresentação de portfólio, não uma regra de negócio do domínio Levante — não entra em `Domain`/`Application` de nenhum bounded context, e não entra no contrato OpenAPI (`levante.json`); logo o gate `dress` de sincronia de contrato não é afetado por esta fatia.
2. **Duas fontes de dados do GitHub**: REST v3 (`api.github.com`) para metadados de repositório, issues abertas e commits recentes; GraphQL v4 (`contributionsCollection`) para o heatmap de contribuições — a API REST não expõe calendário de contribuições de usuário.
3. **Token server-only.** Um Personal Access Token de escopo leitura pública, em variável de ambiente **não-`NEXT_PUBLIC_*`** (nunca chega ao bundle do cliente). Nome escolhido: `GITHUB_API_TOKEN` — deliberadamente diferente de `GITHUB_TOKEN` (que o `.github/workflows/ci.yml` já usa para o token efêmero do runner de Actions, existente só durante o CI) para não confundir os dois secrets. Guardado como qualquer outro segredo do repo: user-secrets/`.env` local, `.env` da VM em produção (mesmo mecanismo do `Hiram:ApiKey`, nunca commitado).
4. **Cache via Next Data Cache (`revalidate` por fetch), mesmo padrão já usado em `sitemap.ts`/`feed.xml/route.ts`.** TTLs: `86400`s (24h) para contribuições (GraphQL, muda devagar, query mais cara) e `3600`s (1h) para repositório/issues/commits (REST). Como os dois tipos de dado aparecem na mesma seção/página, cada fetcher de `lib/github/*` seta seu próprio `{ next: { revalidate: N } }` por chamada — não dá para usar um único `export const revalidate` de rota, que aplicaria um TTL homogêneo à árvore inteira.
5. **Cache frio a cada redeploy é aceitável, sem store persistente.** O `next.config.ts` já usa `output: "standalone"` e o `Dockerfile` não copia `.next/cache` — o Next Data Cache já nasce frio a cada redeploy hoje (nada novo). Com token autenticado (5000 req/h) e a VM rodando **1 réplica** (ADR 0003 — elimina o argumento de cache compartilhado entre instâncias), isso não é um problema real medido; se um dia virar, é dívida a registrar no roadmap, não algo a resolver preventivamente agora.
6. **Resiliência: degradar honesto, nunca inventar dado.** Em erro de rede, rate-limit ou indisponibilidade, cada fetcher retorna `null`/`[]` (mesmo padrão de `try/catch → []` já usado em `sitemap.ts`/`feed.xml`) e a UI mostra um estado discreto ("indisponível no momento"). Diferente do mockup de referência (que cai para dados SEED hardcoded): mostrar número fabricado (stars, commits) num portfólio técnico que vende credibilidade é pior do que mostrar ausência.
7. **Sem rota pública parametrizável.** O conjunto de repositórios é fixo (env-configurado), então a chamada ao GitHub acontece só dentro de Server Components/lib compartilhada — nunca atrás de um Route Handler tipo `/api/github/[owner]/[repo]`, o que abriria um vetor de abuso (variar `owner/repo` na URL forçaria queimar o budget autenticado contra o GitHub a cada valor novo).

## GAP-K (novo) — contas GitHub em aberto

Duas identidades GitHub aparecem nos mockups de referência e no repo, e não está confirmado se são a mesma pessoa:

- `felipeazevedoit` — perfil pessoal usado nos mockups (heatmap de contribuições, "dono exibido" dos cards de portfólio).
- `michel-az-de` — conta técnica/organizacional (`GH_ACCOUNT` em `CLAUDE.md`), dona real do repositório `levante` e onde o CI publica as imagens.

Também não estão confirmados os owners reais de `oracle-pack`, `easystock` e `hiram` para os cards do bento pessoal.

Tudo isso fica **configurável via variável de ambiente** (`GITHUB_PROFILE_ACCOUNT`, `GITHUB_ORG_ACCOUNT`, `GITHUB_SHOWCASE_REPOS`, `GITHUB_LEVANTE_REPO`), com placeholder + `TODO` até o Felipe confirmar — não bloqueia começar a construir a camada de dados (fatia 5), só os valores finais antes de ir ao ar.

**Achado colateral:** `src/web/src/lib/site.ts` já hardcoda o `sameAs` do JSON-LD `Person` apontando para `github.com/michel-az-de` — se as contas forem mesmo distintas, isso deveria apontar para o perfil pessoal (`GITHUB_PROFILE_ACCOUNT`). Ajuste a fazer quando o GAP-K for respondido (fatia 12 do plano).

## Consequências

- Nenhuma mudança em `Domain`/`Application`/`Infrastructure` do `.NET`, nenhum endpoint novo em `Levante.Api`, nenhuma mudança em `levante.json`.
- Nenhuma suíte nova de teste `.NET`/WireMock.NET para esta fatia — testes ficam em Vitest, mockando `fetch`/GraphQL como já se faz para BFF em `publico-route.test.ts`.
- Novo secret `GITHUB_API_TOKEN` a provisionar (local e na VM) antes do card do GitHub sair do estado degradado.
- `docs/blueprint.md` §10 (cliente GitHub em `Portfolio.Infrastructure`) fica registrado como visão superada — não é seguido; esta ADR é a fonte de verdade para E3.

## Alternativas consideradas

- Endpoint novo em `Levante.Api` (ex.: `GET /github/atividade`), cacheado em Mongo: rejeitada — contaminaria o domínio com uma integração de terceiro sem regra de negócio nenhuma, exigiria contrato OpenAPI para dado que não é do Levante, e um hop síncrono a mais (browser/SSR → Next → .NET → GitHub) sem ganho real dado que a VM já roda 1 réplica.
- Route Handler público parametrizável por `owner/repo`: rejeitada — vetor de abuso de rate limit (ver decisão 7).
- Fallback para dados SEED em caso de erro (como o mockup): rejeitada — ver decisão 6.
