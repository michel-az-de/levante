# ADR 0003 — Hospedagem: VM conjunta com o Hiram (Docker Compose)

Status: **Aceito** · Fatia D (lançamento) · jul/2026 · **resolve o GAP-J e supera a decisão provisória de Azure Container Apps**

## Contexto

O `GAP-J` estava registrado como **Azure Container Apps (ACA)**, e todo o **D3** do
[plano de MVP](../plano-mvp-producao.md) foi escrito em torno de ACA/Bicep/Key Vault/OIDC/
Container-Apps-Job — inclusive vários parágrafos só para contornar o *scale-to-zero*.

Duas coisas tornaram essa decisão cara e mal-encaixada:

1. **O relay do outbox é always-on single-replica.** É um `BackgroundService` in-process na API,
   sem lease, que precisa de **exatamente 1 réplica sempre ligada** (a idempotência por `eventId` do
   Hiram torna um POST concorrente seguro, mas escalar exigiria lease antes — ver [ADR 0002](0002-emissao-hiram-http.md)).
   O scale-to-zero do ACA **briga** com isso: o relay para no zero e o cold start machuca a UX de
   engajamento. Rodar ACA com `min=1` e scaling desligado é **contorcer o ACA para virar uma VM
   always-on**, pagando o prêmio gerenciado por algo que o workload não usa.

2. **Já existe uma stack conjunta pronta e revisada** no repo do Hiram
   (`hiram/deploy/stack/`, PR #24), que sobe **Levante + Hiram numa única VM** via `docker-compose`:
   só o **Caddy** é público (80/443, Let's Encrypt automático); o Hiram fica em `127.0.0.1` (nunca
   exposto) e o Levante o chama **in-network** em `http://hiram-api:8080` com `X-Api-Key`;
   observabilidade unificada num único coletor OTLP (`grafana/otel-lgtm`), com trace único
   Levante→Hiram→provider. Há script de provisionamento do tenant e de evidência ponta-a-ponta.

## Decisões

1. **Hospedar Levante e Hiram na mesma VM Linux, via Docker Compose** (`hiram/deploy/stack/`), com o
   **Caddy** como única superfície pública. O Levante emite para o Hiram in-network; nenhuma porta do
   Hiram é publicada.

2. **MongoDB no Atlas externo** (replica set), com **usuário de runtime de privilégio mínimo** — a
   regra de segurança do repo exige, o self-check de boot **aborta em Produção** se o usuário tiver
   role administrativa, e há teste no gate `polish`. Os containers `mongo`/`mongo-rs-init` embutidos
   da stack são **removidos**; o IP público da VM entra no allowlist do Atlas. Co-hospedar **não**
   elimina o custo de banco gerenciado (a economia é ACA/Key Vault/App Insights).

3. **Imagens no GHCR publicadas pelo CI de cada repo.** O CI do Levante passa a buildar/publicar
   **`levante-api` e `levante-web`** (antes só a API), do **mesmo commit**, com `<sha>` + `latest`,
   para que o único `LEVANTE_IMAGE_TAG` da stack resolva as duas imagens. Produção **fixa o `<sha>`**
   (imagem imutável; rollback é re-pin do SHA anterior); nunca roda em `latest`.

4. **CD escopado por serviço e por tag.** O deploy do Levante fixa **só** `LEVANTE_IMAGE_TAG` no
   `.env` da VM e recria **só** `levante-api`/`levante-web`; o do Hiram, só `HIRAM_IMAGE_TAG` e seus
   serviços. A infra compartilhada sobe uma vez no bootstrap. `flock` serializa a janela de escrita do
   `.env`. A stack conjunta continua com **fonte única no repo Hiram** (não vendorizada no Levante).
   Rollback é **manual** (runbook), sem rollback automático — coerente com o "pipeline falha e um
   humano executa o runbook" já aceito no plano.

5. **Segredos em `.env` na VM** (chmod 600, dono = usuário de deploy), não Key Vault — aceite
   consciente para portfólio. Backup **off-host, cifrado e com restore testado** de: keyring de Data
   Protection do Hiram (perdê-lo torna segredos de tenant/provider **indecifráveis**), `.provision-state`,
   dump do Postgres e dump do Mongo.

## Gatilho de reversão

Esta decisão vale **enquanto o Hiram não tiver tenants externos reais** com expectativa de uptime. Se
tiver, o raio de explosão compartilhado (uma VM cai = os dois produtos caem) e a custódia do keyring
do Hiram na VM de portfólio deixam de ser aceitáveis: o Levante sai para VM própria (ou volta ao ACA).
Esse é o único fator que inverte a recomendação.

## Consequências

- **Prós:** custo (uma VM pequena vs. ACA + App Insights + Key Vault + ACR + réplica always-on);
  Hiram sem superfície pública; trace único de graça; o relay always-on é o encaixe natural de VM;
  blueprint pronto/revisado; portabilidade sem lock-in; narrativa de portfólio mais forte.
- **Contras/riscos aceitos:** raio de explosão compartilhado (C1, com o gatilho de reversão acima);
  vizinho barulhento (mitigado por limites de recurso/OOM nos data stores); ops própria (patch de SO,
  Docker, TLS, backups); `.env` mais fraco que Key Vault; keyring do Hiram é load-bearing.
- **Endurecimentos pré-go-live (bloqueadores):** publicar a imagem `levante-web` no CI; usuário Atlas
  de privilégio mínimo (self-check verde em Produção); limites de recurso/OOM nos data stores; backup
  off-host com restore testado do keyring/provision-state/dumps; seed de admin em Produção via opt-in
  `Admin:PermitirSeedEmProducao` (senão não há admin); `ForwardedHeaders` e `SITE_URL` fail-fast;
  auditoria do Caddyfile confirmando que nenhuma UI de gestão (`/mailpit`, rabbit, grafana) é pública.
- **Observabilidade:** OTLP → coletor `otel-lgtm` na VM (Tempo/Loki/Prometheus, Grafana por túnel SSH), com trace único Levante→Hiram→provider. **App Insights não é reintroduzido** (re-adicionaria dependência do Azure, veria só o Levante e mandaria PII ao Azure); fica como fallback só se voltar ao Azure. O app já é OTLP-native (sem código de Azure); dar `mem_limit` ao `lgtm` e alertar `emissoes_falhadas` via Grafana/Alertmanager. Ver D1 do plano de MVP.
- **Docs:** o **D3** do plano de MVP é reescrito para a stack Compose na VM (Bicep/ACA/Key Vault/OIDC/
  Container-Apps-Job saem); o que continua válido permanece (CSP Report-Only, `ForwardedHeaders`,
  noindex provisório, `SITE_URL` em runtime, web em 1 réplica, smoke, rollback em par API/web,
  imagem imutável por SHA).
