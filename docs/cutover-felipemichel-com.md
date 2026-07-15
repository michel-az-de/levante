# Cutover para felipemichel.com — folha operador-facing

Instância **concreta** do [runbook de lançamento](lancamento-runbook.md) para o domínio decidido
(`felipemichel.com`, apex canônico — ver [ADR 0007](adr/0007-dominio-felipemichel-com.md)). O runbook
tem o fluxo canônico e o *porquê*; **aqui ficam os valores reais** para este domínio. Não duplica o
runbook — preenche as lacunas dele.

> **Onde você está.** VM + Hiram já rodam. **Falta**, antes do cutover: (1) cluster MongoDB Atlas de
> produção, (2) conta Resend com `felipemichel.com` verificado, (3) dois ajustes no repo **Hiram**
> (abaixo). Nenhum é código do Levante — são infra/config que você provisiona.

## Passo 0 — pré-requisitos que ainda faltam

| Pré-requisito | O que fazer | Bloqueia |
|---|---|---|
| **MongoDB Atlas (prod)** | Criar cluster (M10+ para backup/PITR gerenciado; ver portão D0.5). Usuário **sem role administrativa** (privilégio mínimo — o boot do `levante-api` aborta em Produção se tiver). Pôr o **IP público da VM** no allowlist do Atlas. | Subir `levante-api`. |
| **Conta Resend** | Verificar o domínio de envio `felipemichel.com` (SPF/DKIM — registros DNS abaixo). Gerar a API key (`re_...`). | Ativar a newsletter (D0 passo 5). |
| **Hiram: redirect `www`** | Adicionar bloco `www` ao `Caddyfile` (abaixo). | `www.felipemichel.com` resolver. |
| **Hiram: flags no compose** | Passar `SITE_INDEXABLE`/`NEWSLETTER_ENABLED` ao `levante-web` (abaixo). | Indexação e newsletter no D0. |

## Passo 1 — ajustes no repo Hiram (PR à parte)

Duas mudanças pequenas na stack (`hiram/deploy/stack/`). São **pré-requisito do cutover** — sem elas,
os passos D0 de indexação/newsletter e o `www` não funcionam.

**a) `Caddyfile` — redirect `www` → apex.** Adicionar, ao lado do bloco `{$SITE_HOST}` existente:

```caddyfile
www.{$SITE_HOST} {
    redir https://{$SITE_HOST}{uri} permanent
}
```

**b) `docker-compose.yml` — plumbing dos flags no serviço `levante-web`.** No bloco `environment:`
do `levante-web`, além do `SITE_URL` que já existe:

```yaml
      # Cutover D0: default off (host provisorio nao indexa; newsletter espera Resend verificado).
      SITE_INDEXABLE: ${SITE_INDEXABLE:-false}
      NEWSLETTER_ENABLED: ${NEWSLETTER_ENABLED:-false}
```

E documentar as duas chaves no `.env.example` da stack (default `false`). Os flags são lidos em runtime
pelo web (`src/web/src/lib/flags.ts`), então ligar = editar o `.env` + `restart`, sem rebuild.

## Passo 2 — DNS no registrador de felipemichel.com

`<IP_DA_VM>` = IPv4 público da VM conjunta. `<IPv6_DA_VM>` só se a VM tiver IPv6.

| Tipo | Nome/Host | Valor | Para quê |
|---|---|---|---|
| `A` | `@` (apex) | `<IP_DA_VM>` | Apex → VM. Caddy emite Let's Encrypt. |
| `AAAA` | `@` (apex) | `<IPv6_DA_VM>` | Opcional (só com IPv6). |
| `A` | `www` | `<IP_DA_VM>` | `www` → VM (o Caddy faz o 301 para o apex). |
| `TXT` | `@` | *(valor do painel Resend, ex.: `v=spf1 include:...resend... ~all`)* | **SPF** (Resend). |
| `TXT`/`CNAME` | `resend._domainkey` (ou o nome que o Resend indicar) | *(valor do painel Resend)* | **DKIM** (Resend). |
| `TXT` | `_dmarc` | `v=DMARC1; p=none; rua=mailto:<seu-email>` | **DMARC** (comece com `p=none`). |

> Os valores exatos de SPF/DKIM **vêm do painel do Resend** ao adicionar `felipemichel.com` — não os
> invente. TLS: o Caddy resolve Let's Encrypt sozinho assim que o apex (e o `www`) apontarem para a VM
> e as portas 80/443 estiverem abertas.

## Passo 3 — `.env` da stack na VM (`hiram/deploy/stack/.env`)

Só os campos que este domínio fixa; o resto segue o [runbook](lancamento-runbook.md#segredos-do-env-da-stack).
Gerar segredos com `openssl rand -hex 24`.

```dotenv
SITE_HOST=felipemichel.com
SITE_URL=https://felipemichel.com
ACME_EMAIL=<seu-email-de-contato-para-o-lets-encrypt>

# Atlas (usuario de privilegio minimo, IP da VM no allowlist)
MONGO_CONNECTION_STRING=mongodb+srv://<usuario>:<senha>@<cluster>.mongodb.net/levante?retryWrites=true&w=majority

# E-mail de producao via Resend (dominio felipemichel.com verificado)
HIRAM_EMAIL_PROVIDER=resend
HIRAM_EMAIL_SECRET=<RESEND_API_KEY>
MAIL_FROM=no-reply@felipemichel.com
LEVANTE_ADMIN_NOTIFICACOES_EMAIL=<seu-email-de-moderacao>@felipemichel.com

# Flags de cutover (default off; ligar nos passos D0.4 e D0.5)
SITE_INDEXABLE=false
NEWSLETTER_ENABLED=false

# Fixar em SHAs revisados no go-live (nunca latest em prod)
HIRAM_IMAGE_TAG=<sha-revisado>
LEVANTE_IMAGE_TAG=<sha-revisado>
```

Os demais segredos (`POSTGRES_PASSWORD`, `RABBITMQ_PASSWORD`, `HIRAM_ADMIN_KEY`, `LEVANTE_JWT_SECRET`,
`LEVANTE_ORIGEM_HASH_SECRET`, `LEVANTE_ADMIN_EMAIL`/`LEVANTE_ADMIN_SENHA`) e o `HIRAM_LEVANTE_API_KEY`
(preenchido **só após** `provision-levante.sh`) seguem o runbook. `chmod 600 .env`.

## Passo 4 — GitHub CD (opcional; hoje inerte)

Só se quiser deploy automático pós-merge. Enquanto `DEPLOY_ENABLED` não for `true`, o deploy é manual
(`./deploy-app.sh levante <sha>` na VM). Para armar:

| Tipo | Nome | Valor |
|---|---|---|
| Environment | `production` | Com protection rules (aprovação manual). |
| Secret | `DEPLOY_SSH_HOST` / `DEPLOY_SSH_USER` / `DEPLOY_SSH_KEY` / `DEPLOY_KNOWN_HOSTS` | Acesso SSH à VM (chave dedicada). |
| Variable | `DEPLOY_ENABLED` | `true` |
| Variable | `SITE_URL` | `https://felipemichel.com` (alvo do smoke pós-deploy). |

> As chaves SSH são suas — configure-as você mesmo no GitHub; elas nunca passam por aqui.

## Passo 5 — sequência do cutover D0 (na VM)

Depois de Atlas + Resend + os dois PRs do Hiram prontos, e o DNS propagado:

1. `.env` com `SITE_HOST`/`SITE_URL` = felipemichel.com (passo 3) → `docker compose up -d` e esperar
   `levante-api`/`levante-web` `healthy`.
2. **Portão D0.5** — backup + **restore testado** do `keyring` do Hiram **antes** de qualquer coisa
   pública (perder o volume `keyring` torna segredos de tenant indecifráveis). Nunca `docker compose down -v` em prod.
3. **Habilitar indexação:** `SITE_INDEXABLE=true` no `.env` → `docker compose up -d levante-web`
   (restart). `robots.txt` libera, `sitemap` sai, `X-Robots-Tag: noindex` some.
4. **Ativar newsletter:** `NEWSLETTER_ENABLED=true` (só com o Resend verificado) → restart do `levante-web`.
5. **Smoke:** `/api/health` + `/` + um GET dinâmico web→API (ver runbook). Conferir `https://felipemichel.com`
   e `https://www.felipemichel.com` (deve 301 para o apex).
6. Registrar `felipemichel.com` no **Google Search Console** + **Bing Webmaster**; submeter o `sitemap.xml`.
7. Escrever os artigos reais em `/admin/artigos/novo` (o banco de produção nasce vazio).

## Referências

- [ADR 0007](adr/0007-dominio-felipemichel-com.md) — a decisão do domínio.
- [runbook de lançamento](lancamento-runbook.md) — fluxo canônico, portões, segredos, rollback.
- `hiram/deploy/stack/README.md` — bring-up da stack, observabilidade, backups.
- `hiram/deploy/levante/README.md` — provisioning do tenant Levante.
