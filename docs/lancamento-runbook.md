# Runbook de lançamento (go-live) do Levante

Runbook **operador-facing**: a sequência concreta para pôr o Levante em produção na VM conjunta com o
Hiram. É o "como fazer" do cutover. O **porquê** e o design de cada decisão ficam no
[plano de MVP](plano-mvp-producao.md); a **ordem das fatias** no [roadmap](roadmap.md); o **bring-up
da stack** (comandos canônicos) no `README.md` de `hiram/deploy/stack/`. Aqui consolida-se e
referencia-se — não se duplica.

> **Veredito de prontidão.** O produto está *code-complete*: publicar artigos, engajamento e
> newsletter (código) já funcionam, CI verde na `main`. O que falta é **cutover de produção
> (Fase D)** — infraestrutura — mais **um ajuste de config no Hiram** (dispatcher hoje aponta pro
> Mailpit, que captura e não entrega). Nenhuma feature nova é necessária para o go-live.

## Decisões fixadas

| Decisão | Estado | Efeito no runbook |
|---------|--------|-------------------|
| **Domínio (GAP-A)** | **`felipemichel.com`** (apex; [ADR 0007](adr/0007-dominio-felipemichel-com.md)) | Cutover **D0**, `www`→301. Tudo via `SITE_URL`/env; nada hardcoded. Valores concretos (DNS/`.env`/CD) em [cutover-felipemichel-com.md](cutover-felipemichel-com.md). |
| **E-mail em produção** | **Resend** (HTTP) | `ResendEmailProvider` já existe no Hiram. Ligar = preencher `.env` do dispatcher (ver PR de parametrização da stack) + conta Resend com **domínio de envio verificado** (SPF/DKIM). |

## Pré-requisitos (uma vez)

1. **Imagens no GHCR** — `levante-api`, `levante-web`, `hiram-api`, `hiram-dispatcher`, publicadas pelos CIs (push na `main`). Escape hatch de build local existe (bloco `build:` no compose).
2. **VM Linux** com Docker + Compose, e `hiram/deploy/stack/` (+ `deploy/levante/`) em **`/opt/levante-hiram/`** (caminho fixo no CD — ver `.github/workflows/ci.yml`).
3. **MongoDB Atlas** de produção: cluster criado, **usuário de privilégio mínimo** (sem role administrativa — o boot do `levante-api` aborta em Produção se tiver), **IP público da VM no allowlist**. Tier: M10+ para backup/PITR gerenciado (ver [portão D0.5](#portões-que-bloqueiam-o-go-live)).
4. **Conta Resend**: domínio de envio **verificado** (SPF/DKIM), API key gerada.

## Sequência de go-live

Segue o [bring-up](../../hiram/deploy/stack/README.md) da stack, com os pontos específicos do Levante
destacados. Ordem:

1. **`.env` da stack** — `cp .env.example .env`, `chmod 600 .env`, preencher **todos os segredos EXCETO `HIRAM_LEVANTE_API_KEY`** (ver [tabela](#segredos-do-env-da-stack)). Essa key só existe depois do passo 3.
2. **Subir a stack** — `docker compose up -d`. Esperar `hiram-migrate`/`keyring-init` saírem com exit 0 e api/web/dispatcher `healthy`.
   > O compose **recusa subir `levante-api`** enquanto `HIRAM_LEVANTE_API_KEY` estiver vazio (guard `:?` em `docker-compose.yml`). Isso é esperado — o serviço sobe no passo 3.
3. **Provisionar o tenant Levante no Hiram (1×)** — por túnel SSH ao `hiram-api` (`127.0.0.1:8080`):
   ```bash
   HIRAM_BASE_URL=http://127.0.0.1:8080 HIRAM_ADMIN_KEY=<HIRAM_ADMIN_KEY do .env> \
     ../levante/provision-levante.sh
   ```
   Copie a API key impressa (`hk_live_…`, **mostrada uma única vez**) para `HIRAM_LEVANTE_API_KEY` no `.env` e recrie o serviço: `docker compose up -d levante-api`.
   Cria tenant + templates + routines (mapa evento→template em `deploy/levante/README.md`).
4. **[Portão D0.5 — backup do keyring](#portões-que-bloqueiam-o-go-live)** antes de qualquer coisa pública.
5. **CD no GitHub** — configurar o repositório do Levante (ver [tabela](#secrets-e-variables-do-github-cd)). Enquanto `DEPLOY_ENABLED` não for `true`, o job `deploy` fica **inerte** (deploy é manual via `deploy-app.sh levante <sha>`).
6. **Smoke** — `./evidence/run.sh` (conferir A4/A6/A7) + o smoke do pipeline (`/api/health` + `/`).
7. **D1 no Grafana** (na VM, por túnel SSH) — fixar a retenção do Loki e criar o alerta de `emissoes_falhadas` (gatilho de rollback). O app já emite OTLP para o `lgtm`.
8. **Cutover D0** — [domínio](#cutover-d0--domínio).
9. **Escrever os artigos reais** em `/admin/artigos/novo` (o banco de produção nasce vazio; só há seed em dev).

## Segredos do `.env` da stack

Fonte: `hiram/deploy/stack/.env.example` (todos `CHANGE_ME`). Gerar com `openssl rand -hex 24`.

| Chave | O que é |
|-------|---------|
| `POSTGRES_PASSWORD`, `RABBITMQ_PASSWORD` | Infra do Hiram. |
| `HIRAM_ADMIN_KEY` | `X-Admin-Key` do Hiram (provisioning). |
| `LEVANTE_JWT_SECRET` | Assinatura do JWT do admin (**≥32 chars**). |
| `LEVANTE_ORIGEM_HASH_SECRET` | HMAC do hash de origem anti-abuso (**≥32 chars**). |
| `LEVANTE_ADMIN_EMAIL` / `LEVANTE_ADMIN_SENHA` | Seed do 1º admin (opt-in de seed em prod já embutido). |
| `LEVANTE_ADMIN_NOTIFICACOES_EMAIL` | Destino do aviso de comentário pendente. |
| `MONGO_CONNECTION_STRING` | Atlas srv URI, usuário de privilégio mínimo. |
| `SITE_HOST` / `SITE_URL` / `ACME_EMAIL` | Host público + TLS (Caddy/Let's Encrypt). `felipemichel.com` (valores na [folha de cutover](cutover-felipemichel-com.md)). |
| `MAIL_FROM` | Remetente exibido; com Resend, do domínio verificado. |
| **Campos do Resend** | Provider + secret + `from` do dispatcher (ver PR de parametrização da stack). |
| `HIRAM_LEVANTE_API_KEY` | **Preenchido só após o passo 3** (provision). |
| `HIRAM_IMAGE_TAG` / `LEVANTE_IMAGE_TAG` | Fixar em SHAs revisados no go-live (nunca `latest` em prod). |

## Secrets e variables do GitHub (CD)

Hoje o repositório do Levante tem **zero** secrets/variables → o job `deploy` está inerte (por design). Para ligar o CD:

| Tipo | Nome | Valor |
|------|------|-------|
| Environment | `production` | Com protection rules (aprovação). |
| Secret | `DEPLOY_SSH_HOST` / `DEPLOY_SSH_USER` / `DEPLOY_SSH_KEY` / `DEPLOY_KNOWN_HOSTS` | Acesso SSH à VM (chave dedicada). |
| Secret | `DEPLOY_SSH_PORT` | Opcional (default 22). |
| Variable | `DEPLOY_ENABLED` | `true` para armar o job. |
| Variable | `SITE_URL` | Alvo do smoke pós-deploy (`/api/health` + `/`). |

> GHCR sai de graça via o `GITHUB_TOKEN` embutido — não precisa de secret.

## Portões que bloqueiam o go-live

Não são itens opcionais de checklist:

- **D0.5 — backup + restore do `keyring` TESTADO** antes do primeiro artigo público / DNS final.
  Perder o volume `keyring` (Data Protection do Hiram) torna segredos de tenant/provider
  **indecifráveis** → re-provisão forçada. Backup off-host cifrado do `keyring` +
  `deploy/levante/.provision-state` + dumps (Postgres), com **restore validado em cluster/scratch**.
  O Mongo é Atlas (backup/PITR gerenciado no tier M10+; se optar por M0/M2, agende um `mongodump`
  como script, não como intenção). **Nunca `docker compose down -v` em produção.**

  > **Mídia de artigo mora no Mongo (GridFS).** Desde a fatia de autoria rica, imagem de artigo é
  > gravada no bucket `midias` do próprio banco ([ADR 0008](adr/0008-midia-gridfs.md)), e não em
  > storage à parte. Duas consequências para este portão:
  > **(1)** o dump passa a arrastar os binários — refaça a medição da janela de backup/restore
  > contando com eles, não com o tamanho de quando era só texto;
  > **(2)** perder o banco agora perde também as imagens dos artigos, que não são recuperáveis do
  > markdown (o corpo guarda só `/midias/{id}`).
  > Se a produção estiver com **Mongo self-hosted** na VM (e não Atlas), o `mongodump` agendado é
  > obrigatório, não opcional — e vale **monitorar o disco**: a VM é compartilhada com o Hiram, então
  > disco cheio derruba os dois produtos. O volume cresce por design nas próximas fatias (o editor
  > sobe uma imagem a cada colagem, inclusive de rascunho descartado) e ainda **não há coleta de
  > mídia órfã**.
- **Smoke automatizado** como gate do pipeline (`/api/health` + `/` + — TODO(infra) — 1 GET dinâmico web→API pela rota pública de reações).
- **Verificação visual** — rodar `npm run dev` (Node 20+) e conferir `/`, `/levante`, `/artigos`, tema/idioma/⌘K, ou verificar contra a URL pós-deploy. (Nunca foi feita: o Node local do dev é 18, abaixo do mínimo do Next 16.)

## Cutover D0 — domínio

Domínio **decidido**: `felipemichel.com` (apex canônico, `www`→301; [ADR 0007](adr/0007-dominio-felipemichel-com.md)). Os
**valores concretos** deste domínio (registros DNS, `.env` da stack, secrets do CD, e os dois ajustes pré-requisito no repo
Hiram) estão em [cutover-felipemichel-com.md](cutover-felipemichel-com.md). Sequência (tudo via `SITE_URL`, sem hardcode):

1. Domínio comprado (`felipemichel.com`).
2. DNS + TLS (apontar `SITE_HOST=felipemichel.com`; o Caddy emite Let's Encrypt para apex e `www`).
3. Atualizar `SITE_URL`/`SITE_HOST` no `.env` → **restart + `revalidate` completo** do web (OG, JSON-LD, RSS, sitemap "assam" a URL).
4. **Habilitar indexação** — setar `SITE_INDEXABLE=true` (`robots.txt` libera, `sitemap` sai, `X-Robots-Tag: noindex` some). Antes disso o host provisório fica fora do índice — flag explícito, não inferência por host (o `SITE_URL` interino é o próprio host provisório).
5. **Ativar a newsletter** (flag `NEWSLETTER_ENABLED`) — só agora, com o domínio de envio Resend verificado (senão o `confirmUrlBase` aponta para o host provisório e/ou queima a reputação do domínio novo).
6. Registrar no Google Search Console + Bing Webmaster.

## Código: pronto vs. pendente

| Item | Estado |
|------|--------|
| Pipeline de artigos (backend + admin UI + leitura pública) | **Pronto** |
| Engajamento (reações + comentários/moderação) | **Pronto** |
| Newsletter double opt-in (código) | **Pronto** (entrega depende do Hiram em prod) |
| Health `/health/live` + `/health/ready`, OpenTelemetry/OTLP | **Pronto** |
| `SITE_URL` fail-fast, `ForwardedHeaders`, seed de admin opt-in, `.env.example` | **Pronto** |
| Dockerfiles (api/web) + CI `raise` (imagens no GHCR) + job `deploy` (inerte) | **Pronto** |
| `/sobre`, `error.tsx` global, `LINKEDIN_URL` no JSON-LD, conteúdo da política | Fecha na PR de D2 |
| CSP `Report-Only` + endpoint de report, supressão de indexação no host provisório, auditoria `SITE_URL` runtime, flag `NEWSLETTER_ENABLED` | Fecha na PR de D3 |
| Provider de e-mail do Hiram Mailpit→Resend (parametrização + `.env`) | Fecha na PR da stack (repo Hiram) + conta Resend |
| Retenção do Loki, alerta `emissoes_falhadas` | **D1 na VM** (Grafana) — operação |
| VM, Atlas, DNS/TLS, secrets, provision, backups | **Operação** (você) |

## Rollback

Manual, em **par API/web** por SHA (o web assa o contrato da API no build). Procedimento e gatilhos em
`plano-mvp-producao.md` (checklist item 12) e no `README.md` da stack (`deploy-app.sh <app> <sha-anterior>`).
Rollback cobre código, não dados (disciplina forward-only para schema).

## Referências

- [plano-mvp-producao.md](plano-mvp-producao.md) — design, checklist de go-live, riscos aceitos.
- [roadmap.md](roadmap.md) — ordem das fatias (Fase D = go-live).
- `hiram/deploy/stack/README.md` — bring-up canônico, observabilidade, backups.
- `hiram/deploy/levante/README.md` — provisioning do tenant, mapa evento→template.
- ADRs [0002](adr/0002-emissao-hiram-http.md) (HTTP/Hiram), [0003](adr/0003-hospedagem-vm-conjunta-hiram.md) (VM conjunta).
