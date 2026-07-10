# Plano de MVP em produção, Levante

Plano operacional da reta final: o que já está pronto, o que falta até o **go-live** e até o **MVP completo**, e as decisões de produção que travam o deploy se não forem tomadas antes. Complementa o [roadmap](roadmap.md) (que é a ordem de execução) e o [blueprint](blueprint.md) (que é a visão). Onde houver critério de "pronto" por fatia, ele vive no roadmap; aqui não se duplica, referencia-se.

## Dois marcos, sem ambiguidade

| Marco | O que significa | Fatia que fecha |
|-------|-----------------|-----------------|
| **Go-live** | Site no ar no domínio final: conteúdo público + admin + engajamento, com observabilidade e LGPD base. Newsletter condicionada à prontidão do Hiram (ver [Arestas do Hiram](#arestas-do-hiram)). | Fim de **D3** |
| **MVP completo** | O site "te vendendo": leads + portfólio + integração GitHub, **iterados já em produção**. | Fim de **E3** |

O gate de produção é **D3**. A Fase E não espera um segundo ambiente — itera direto em produção sob environment protection (ver [Riscos residuais](#riscos-residuais-aceitos)).

## Estado atual (verificado)

Fases A, B e C entregues. Evidência direta no repositório (não inferência):

| Fatia | Situação | Evidência |
|-------|----------|-----------|
| A1 Higiene de CI | Entregue | `.github/workflows/ci.yml`: split `Category!=Integration`/`=Integration`; cobertura como **gate** (`Threshold=80`, `ThresholdType=line`, `ThresholdStat=minimum`) por assembly nos 4 contextos |
| A2 Harness de testes do front | Entregue | 13 arquivos `*.test.ts(x)` em `src/web/src`; `npm test` no job `polish` |
| A3 Contrato de erro Result→HTTP | Entregue | `tests/Levante.Api.IntegrationTests/ResultadoHttpTests.cs` (status por tipo de `Error` + ProblemDetails) |
| A4 MongoOptions compartilhado | Entregue | `src/api/shared/Levante.SharedKernel.Infrastructure/MongoOptions.cs` |
| A5 Admin JWT em cookie httpOnly + CSRF | Entregue | `src/web/src/app/api/admin/sessao/route.ts`; proxy admin com 403 em origem cruzada (`.../admin/proxy/[...caminho]/route.ts`) |
| B1 Reações anônimas | Entregue | Contexto `Engajamento` (`Reacao`, endpoints com rate limit, `ReacoesArtigo.tsx` + teste) |
| B2 Comentários + moderação | Entregue | `Comentario` (Pendente→Aprovado/Rejeitado), honeypot, `/admin/comentarios`, testes |
| C1 Outbox transacional + relay | Entregue | `RelayDeOutbox` (flag-based, backoff por evento) |
| C2 Newsletter double opt-in | Entregue | Agregado `Assinante`, token opaco, consentimento com timestamp |
| C3 Notificação de comentário pendente | Entregue (código) | `TiposDeEvento.ComentarioPendente`, `MapeadorDeEmissao`; **entrega fim-a-fim depende do Hiram em produção** |

Base de testes do backend: 214 `[Fact]`/`[Theory]` em 6 projetos. CI verde na `main`.

Achados que orientam as decisões de produção abaixo:
- O claim do relay **não é atômico** (`Find` + `UpdateOne`): sob concorrência (múltiplas réplicas) há emissão duplicada — seguro com 1 réplica na VM. Se o processo do relay parar (deploy/restart), o outbox acumula até voltar.
- `Idempotency-Key` = `eventId` = id da linha do outbox (dedupa retry sem colapsar eventos distintos) — correto no lado Levante; falta validar em produção que o **Hiram** persiste e respeita a chave.
- A métrica `emissoes_falhadas` existe como counter OTel, mas sem export/alerta até **D1**.
- Health probes `/health/live` e `/health/ready` já existem na API; o web ainda não tem healthcheck.
- Índices Mongo são criados no bootstrap por `*InicializacaoHostedService` (confirmar em prod, não recriar).
- O e-mail de confirmação do double opt-in trafega **via Hiram** (`assinatura_solicitada` carrega `token` + `confirmUrlBase`).

## Plano de execução

Ordem: **D1 → D2 → D3 (com o marco D0 no meio) → E1 → E2 → E3**. Observabilidade sobe junto com o primeiro deploy; a Fase E roda já em produção.

### D1 — Observabilidade mínima

Logs estruturados em JSON, export OTel (OTLP) → coletor `otel-lgtm` na VM (traces em Tempo, logs em Loki, métricas em Prometheus; Grafana por túnel SSH), com trace único Levante→Hiram→provider. O app já emite OTLP (`OTEL_EXPORTER_OTLP_ENDPOINT`), então é só apontar para `http://lgtm:4317` — sem SDK de Azure. Antes de somar Serilog como segundo stack, avaliar o caminho enxuto: OTel logs + formatter JSON no console.

- Ligar os probes existentes (`/health/live`, `/health/ready`) no **healthcheck do compose** com **`retries`/intervalo tolerantes a blips curtos** do Atlas — readiness afirma Mongo, e sem essa folga um blip vira flapping de restart. O healthcheck do web usa a rota `/api/health` (**feita**).
- **Request logging cria coleta nova de dado pessoal** (IPs, user-agents na telemetria): fica na VM (Loki no `lgtm`), não sai para terceiro. D1 **fixa o período de retenção** (retention do Loki — escolher e registrar) e avalia mascarar o IP no processor OTel antes do export, para simplificar a base legal. Esse número é escrito na política em D2.
- A partir daqui `emissoes_falhadas` tem export/alerta (Grafana/Alertmanager sobre a métrica no `lgtm`) — pré-requisito do trigger de rollback.

### D2 — Vitrine de identidade + LGPD

`/sobre`, `wa.me` (click-to-chat) e `/politica-privacidade`.

- A política cobre **o que já se coleta hoje**: comentários (nome, conteúdo), e-mail de assinante, IP usado em rate limiting e no hash de origem, logs/telemetria com o período de retenção fixado em D1 (base legal por item: interesse legítimo vs consentimento). E **descreve prospectivamente os leads** (UTM/origem) para não nascer defasada quando E1 entrar — E1 tem passo de revisão da política.
- Encaixes de confiabilidade do front: `error.tsx` global (500 gracioso) e `LINKEDIN_URL` no JSON-LD `Person`.

### D3 — Deploy na VM conjunta

Stack Compose na VM do Hiram ([ADR 0003](adr/0003-hospedagem-vm-conjunta-hiram.md)): imagens `levante-api`+`levante-web` no GHCR, MongoDB Atlas de produção (privilégio mínimo), CORS/CSP de produção, DNS/TLS via **Caddy** (Let's Encrypt automático), CD escopado pós-`raise` com environment protection, Search Console. Meta de headers: **A no securityheaders.com exceto a CSP** (ver decisão abaixo).

**Acesso de rede ao Atlas.** A VM tem IP público fixo, então o allowlist do Atlas é o **IP da VM** (não `0.0.0.0/0`) + TLS + auth forte + conta least-privilege. Hardening: **Private Endpoint/PrivateLink** se a nuvem da VM suportar. A conta de runtime é **de privilégio mínimo** (sem role administrativa): o self-check de boot aborta em Produção se não for, e há teste no gate `polish`. Os containers `mongo`/`mongo-rs-init` embutidos da stack são removidos (Mongo fica externo no Atlas). O deploy trava no primeiro connect se allowlist/usuário não existir.

**Tier do Atlas.** Backup automático + PITR exige **M10+** — é o piso de custo que o checklist de backup/restore assume. Alternativa mais barata (M0/M2 + snapshot/`mongodump`) rebaixa a garantia e, se adotada, precisa ser dita.

**Topologia do relay do outbox.** Na VM o relay é o `BackgroundService` in-process da `levante-api`, com **1 réplica sempre ligada** (`restart: unless-stopped`) — a forma natural do workload (sem scale-to-zero para o relay parar, sem cold start; latência quase-zero). Como é single-writer por construção (1 réplica, sem `deploy.replicas>1`) e a idempotência por `eventId` do Hiram cobre um POST concorrente, o claim `Find`+`UpdateOne` atual é seguro aqui. **Escalar além de 1 réplica exige antes tornar o claim atômico** (`FindOneAndUpdate` com filtro de status + lease/timeout) — follow-up, ver [ADR 0002](adr/0002-emissao-hiram-http.md).

**CSP.** No go-live a CSP vai como **`Content-Security-Policy-Report-Only`**, com endurecimento para enforcement como item pós-D3. Motivo: nenhuma CSP estrita sobrevive à arquitetura SSG/ISR atual. Nonce por request força dynamic rendering global e mata o cache do blog. Hash-based quebra no ISR — o payload RSC é inlinado (`self.__next_f.push(...)`) e a primeira revalidação regenera inlines cujos hashes não estão na CSP assada no build. A escolha é binária: nota A agora (dynamic global) **ou** CSP correta sem matar o ISR — o MVP escolhe a segunda. O Report-Only exige `report-to`/`report-uri` apontando para um **route handler próprio que loga o JSON da violação** (App Insights não recebe CSP reports nativamente); sem endpoint, as violações morrem no console dos visitantes e o endurecimento nunca tem dados. A CSP já antecipa as fontes de E2/E3 (imagens/avatares do GitHub em `img-src`, GitHub API em `connect-src` do server).

**IP real atrás do Caddy.** `ForwardedHeadersOptions` com **`ForwardLimit = 1`** (confiar só no último hop) e **`KnownIPNetworks`/`KnownProxies` limpos**, ativado **só fora de Development** (honrar `X-Forwarded-For` sem proxy confiável na frente permitiria spoof em dev/testes). **Feito** em `Program.cs`. Ressalva: o caminho público é browser→Caddy→web(BFF)→api, então o rate limit por-cliente real ainda exige o BFF do web propagar o `X-Forwarded-For` do cliente (follow-up); sem isso os buckets de rate limit por IP agregam no IP do container web.

**noindex no host provisório.** `X-Robots-Tag: noindex` **condicional ao header `Host`** (`*.azurecontainerapps.io`), via middleware — a mesma app serve o domínio final pós-cutover, e um header fixo vazaria para ele.

**`SITE_URL` em runtime.** Alvo: server-only lido em runtime, para que D0 seja **restart + revalidate** (não rebuild) e a imagem do web seja promovível entre host provisório e domínio final (rollback simétrico). A auditoria vai além de `NEXT_PUBLIC_*`: SSG lê env no build, e **route handlers (`sitemap.ts`, RSS, OG image) são estáticos por default** no App Router — verificar cada um e cada página `force-static` quanto a revalidação (marcar `force-dynamic` ou incluir no `revalidatePath` do D0). Se algum uso ficar build-time, o rollback do web fica limitado à primeira revisão pós-D0.

**Web em 1 réplica no MVP** (`min=max=1`). O cache ISR do Next é filesystem por instância: `revalidatePath` atinge só a réplica que recebeu o request, as outras servem stale até o TTL. O fluxo "publicar no admin → revalidar o blog" só é determinístico com 1 réplica ou `cacheHandler` compartilhado (Redis, fora do MVP). Amarra a dívida "revalidate ISR centralizado", que passa a ser "cacheHandler compartilhado quando escalar".

**Fecho de D3:** fail-fast de `SITE_URL` no boot do container (**feito** em `src/web/src/instrumentation.ts`), `.env.example` do web (existe) e da API (`src/api/.env.example`, **feito**), seed de admin em Produção via `Admin:PermitirSeedEmProducao` (**feito**), limites de recurso/OOM nos data stores da stack (`hiram/deploy/stack`), confirmação dos índices Mongo em produção.

#### Marco D0 — domínio (dentro de D3)

A decisão do domínio (GAP-A) acontece **durante** D3, não a bloqueia: todo o trabalho anterior usa `SITE_URL` via env. Sequência do cutover: decidir/comprar → DNS/TLS → restart + **revalidate completo** do web (OG, JSON-LD, RSS, sitemap assam a URL) → remover o noindex provisório → ativar o form da newsletter (se in-scope) → registrar no Search Console/Bing.

### E1 — Leads

Form + mini-CRM no contexto Audiencia + evento `NovoLead` via Hiram (UTM/origem no agregado). Anti-spam pelo mesmo molde de newsletter/comentários: bucket de rate limit público (20/min/IP) + honeypot. Revisar a política de privacidade e a CSP.

### E2 — Portfólio

Agregado `Projeto` + `/projetos` (admin + SSG + JSON-LD + sitemap), no molde de Conteudo. Revisar a CSP (imagens de projeto).

### E3 — Integração GitHub

GraphQL (repos, contribuições) com cache TTL/ISR e degradação graciosa; webhook de invalidação permanece TODO. Revisar a CSP (`img-src`/`connect-src` do GitHub).

## Arestas do Hiram

O go-live com newsletter funcional depende do Hiram em produção. Três arestas explícitas:

1. **Dedup é load-bearing, inclusive para concorrência.** O claim atômico com lease reduz a probabilidade de emissão concorrente, **não a elimina** (lease expirado com o worker original ainda vivo — pausa de GC, Hiram lento, rede — é lease sem fencing): at-least-once permanece e o dedup a jusante continua obrigatório. Validar em produção que o Hiram persiste/respeita a `Idempotency-Key` **e por quanto tempo a retém** (a janela precisa cobrir `lease + backoff máximo de retry`).
2. **Replay não pode disparar confirmações fantasmas.** `assinatura_solicitada` com mais de **7 dias** recebe status terminal próprio (`Expirada`) na mesma passada do relay — não basta filtrar, ou a linha fica `Pendente` para sempre. Religar o relay após semanas não pode mandar "confirme sua inscrição" a quem se inscreveu há muito tempo. Eventos internos velhos (`comentario_pendente`) são aceitos sem expiração — notificação ao admin, sem impacto no usuário.
3. **Fallback honesto.** Se o Hiram não estiver pronto no go-live, a newsletter **sai do escopo do go-live** (form oculto/desabilitado por flag) ou entra em estado degradado com mensagem explícita — nunca um form no ar coletando e-mail que jamais confirma. Em qualquer caso o form só é ativado **após o marco D0** (o `confirmUrlBase` embutido apontaria para o host provisório antes do cutover).

Semântica de disponibilidade: **`/health/ready` afirma Mongo, não Hiram** — incluir o Hiram acoplaria a disponibilidade do Levante à de um sistema externo.

## Checklist de go-live

1. Decidir newsletter in/out conforme prontidão do Hiram (form ativado sempre pós-D0).
2. Atlas de produção: tier definido, modelo de acesso de rede decidido, conta least-privilege.
3. `.env` da stack na VM (chmod 600, dono = usuário de deploy) + secrets; backup **off-host cifrado** do keyring do Hiram, `.provision-state` e dumps (Postgres/Mongo).
4. Pipeline publica **imagens imutáveis** (`levante-api`+`levante-web`, do mesmo commit) taggeadas por SHA (nunca `latest` em produção) — fundação do rollback; produção fixa o `<sha>` no `LEVANTE_IMAGE_TAG`, com `latest` o re-pin viraria no-op silencioso.
5. Deploy GitHub Actions → VM via **SSH** (chave dedicada, `known_hosts` fixado, environment protection), escopado por serviço/tag; inerte até a variável `DEPLOY_ENABLED`.
6. Deploy na VM (Compose): `levante-api` (relay in-process, 1 réplica) + `levante-web` (1 réplica), atrás do Caddy. O deploy do Levante recria **só esses 2 serviços** (Hiram/Postgres/Caddy intactos).
7. **Smoke automatizado como step do pipeline**: `/health/ready` + 2–3 páginas públicas + **1 chamada dinâmica não autenticada que atravessa o contrato web→API** (a rota pública de reações que o `ReacoesArtigo.tsx` consome — não o proxy admin, que exige sessão). Páginas SSG passam de cache mesmo com a API incompatível; só a chamada dinâmica pega o cenário "par API/web quebrado".
8. Marco D0 (domínio, DNS/TLS, revalidate, remover noindex, ativar newsletter).
9. Search Console + Bing Webmaster.
10. Verificar headers: A exceto CSP (que está em Report-Only).
11. Backup automático + **restore testado em cluster scratch** (nunca em produção).
12. **Rollback definido.** Gatilho: smoke falho, 5xx sustentado, `emissoes_falhadas` disparando (esse último só vale pós-D1). Procedimento no MVP: **o pipeline falha e um humano executa o runbook** (sem rollback automático condicional). Reverter **em par API/web** (o web assa o contrato da API no build; reverter um lado só exige checar compatibilidade), re-pinando o `LEVANTE_IMAGE_TAG` no SHA anterior + `up -d levante-api levante-web`. O **relay volta junto com a API** (é in-process na `levante-api`), sem procedimento separado. **Rollback cobre código, não dados**: disciplina forward-only para qualquer mudança de schema/collection nas fatias E.

## Riscos residuais aceitos

- **Admin com fator único** (senha + lockout + rate limit). MFA TOTP é pós-MVP.
- **Dependência operacional do Hiram** para o caminho de e-mail (mitigada pela flag de fallback).
- **Sem staging**: a Fase E itera direto em produção sob environment protection — aceitável para leads/portfólio, barateado pelo smoke automatizado.
- **Relay at-least-once dedupado a jusante** (lease sem fencing).
- **VM compartilhada com o Hiram** ([ADR 0003](adr/0003-hospedagem-vm-conjunta-hiram.md)): uma VM cai = os dois produtos caem; vizinho barulhento mitigado por limites de recurso/OOM nos data stores. Reversão da decisão se o Hiram ganhar tenants externos.
- **Rate limit in-memory**: reseta a cada restart/deploy do container e é por réplica (1 réplica na VM = global por instância). Sem scale-to-zero na VM, não há cold start no engajamento.

## Fora de escopo do MVP

Analytics first-party + banner de consentimento · dashboard admin · `/tag/[slug]` · Documents (bloqueado por GAP-B/GAP-C) · tradução do corpo de artigo (i18n de conteúdo — distinto do chrome bilíngue PT/EN, já reaberto via GAP-H/ADR 0005) · MFA TOTP. Tudo isso é evolução contínua pós-MVP, sem compromisso de escopo.
