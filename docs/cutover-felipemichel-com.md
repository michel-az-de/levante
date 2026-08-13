# Cutover para felipemichel.com — folha operador-facing

Instância **concreta** do [runbook de lançamento](lancamento-runbook.md) para o domínio decidido
(`felipemichel.com`, apex canônico — ver [ADR 0007](adr/0007-dominio-felipemichel-com.md)). O runbook
tem o fluxo canônico e o *porquê*; **aqui ficam os valores reais** para este domínio. Não duplica o
runbook — preenche as lacunas dele.

> **Onde você está.** VM + Hiram já rodam. **Falta**, antes do cutover: (1) cluster MongoDB Atlas de
> produção, (2) provider `twilio-email` provisionado para o tenant do Levante, com remetente em
> `felipemichel.com` autenticado no Twilio, (3) dois ajustes no repo **Hiram** (abaixo). Nenhum é código
> do Levante — são infra/config que você provisiona.
>
> **Este documento tem uma segunda desatualização, ainda não corrigida:** as menções a **MongoDB Atlas**
> deixaram de valer com o [ADR 0009](adr/0009-hospedagem-vps-stacks-separadas.md), que trocou o Atlas por
> MongoDB self-hosted na própria máquina. Fica em issue própria, para não misturar duas reconciliações na
> mesma entrega.

## Passo 0 — pré-requisitos que ainda faltam

| Pré-requisito | O que fazer | Bloqueia |
|---|---|---|
| **MongoDB Atlas (prod)** | Criar cluster (M10+ para backup/PITR gerenciado; ver portão D0.5). Usuário **sem role administrativa** (privilégio mínimo — o boot do `levante-api` aborta em Produção se tiver). Pôr o **IP público da VM** no allowlist do Atlas. | Subir `levante-api`. |
| **Provider `twilio-email`** | Usar a conta Twilio **que já serve SMS e WhatsApp** (ADR-028 do Hiram) — não se cria conta de SendGrid nem de Resend. Autenticar o domínio de envio `felipemichel.com` no Twilio (DKIM/SPF — ver Passo 2). Provisionar com `PUT /v1/providers/email` no Hiram (Passo 3). | Ativar a newsletter (D0 passo 5). |
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
      # Cutover D0: default off (host provisorio nao indexa; newsletter espera o provider de e-mail pronto).
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
| `CNAME` | `www` | `felipemichel.com.` | **Já existe na zona.** Segue o apex sozinho — ver aviso abaixo. |
| `TXT` | `@` | **editar o SPF existente**, acrescentando o `include` do Twilio | **SPF** — ver aviso abaixo. |
| `CNAME` | *(nomes que o Twilio indicar)* | *(valores do painel Twilio)* | **DKIM** do domínio autenticado. |
| `TXT` | `_dmarc` | **já existe**, em `p=quarantine` | **DMARC** — ver aviso abaixo. |

> **Três armadilhas, todas medidas na zona real em 2026-08-13.** A zona não estava vazia, e tratá-la
> como se estivesse quebra coisa que já funciona.
>
> 1. **`www` já é um `CNAME` para o apex**, não um registro `A`. Não crie um `A www`: ele passaria a
>    duplicar a verdade em dois lugares, e um cutover futuro que trocasse só o apex deixaria o `www`
>    apontando para o servidor velho.
> 2. **Já existe SPF** (`v=spf1 include:spf.em.secureserver.net ?all`, do e-mail da GoDaddy). O `include`
>    do Twilio tem que ser **acrescentado a esse registro**. Criar um segundo TXT `v=spf1` é falha
>    permanente por definição — mais de um SPF na mesma zona invalida os dois, e derrubaria junto o envio
>    que já existe.
> 3. **O DMARC está em `p=quarantine`**, não em `p=none`. Não há período de tolerância: se SPF ou DKIM não
>    alinharem, o primeiro e-mail já vai para quarentena em vez de apenas ser marcado. Conferir o
>    alinhamento antes de declarar o canal pronto.
>
> Os valores exatos de DKIM **vêm do painel do Twilio** ao autenticar `felipemichel.com` — não os invente.
> TLS: o Caddy resolve Let's Encrypt sozinho assim que o apex (e o `www`) apontarem para a VM e as portas
> 80/443 estiverem abertas.

## Passo 3 — `.env` da stack na VM (`hiram/deploy/stack/.env`)

Só os campos que este domínio fixa; o resto segue o [runbook](lancamento-runbook.md#segredos-do-env-da-stack).
Gerar segredos com `openssl rand -hex 24`.

```dotenv
SITE_HOST=felipemichel.com
SITE_URL=https://felipemichel.com
ACME_EMAIL=<seu-email-de-contato-para-o-lets-encrypt>

# Atlas (usuario de privilegio minimo, IP da VM no allowlist)
MONGO_CONNECTION_STRING=mongodb+srv://<usuario>:<senha>@<cluster>.mongodb.net/levante?retryWrites=true&w=majority

# E-mail de producao: NAO se configura aqui. O provider e por TENANT, nao por ambiente.
# Ver o bloco "Provider de e-mail do tenant" logo abaixo desta secao.
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

### Provider de e-mail do tenant (não é `.env`)

O ADR-028 do Hiram grava o provider **por tenant**, em `tenant_provider_configs`, e não por variável de
ambiente. O default de plataforma (`smtp`→Mailpit) continua valendo para quem não tem config própria, e
Mailpit **captura sem entregar** — então enquanto este passo não for feito, nenhum e-mail sai de verdade.

A chamada é um upsert, então rotacionar a credencial depois é exatamente ela de novo:

```
PUT /v1/providers/email
X-Api-Key: <HIRAM_LEVANTE_API_KEY>

{"provider":"twilio-email","settings":{"from":"no-reply@felipemichel.com","api_key_sid":"SK..."},"secret":"<api key secret>"}
```

Para e-mail o `settings` leva só `from` e `api_key_sid` — sem `account_sid`, ao contrário de SMS e WhatsApp.
Mande o corpo por **stdin**, nunca em argv: o argv de um processo em execução é legível por outros processos
do host, e o segredo está nele. O `deploy/jornada/provision.sh` do Hiram é o exemplo pronto dessa chamada.

**O `secret` é cifrado com o keyring de Data Protection do Hiram.** A partir deste passo, perder o volume
`hiram_keyring` torna a credencial indecifrável e exige reprovisionar — é o que o portão D0.5 protege.

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

Depois do banco, do provider `twilio-email` provisionado e dos dois PRs do Hiram prontos, e o DNS propagado:

1. `.env` com `SITE_HOST`/`SITE_URL` = felipemichel.com (passo 3) → `docker compose up -d` e esperar
   `levante-api`/`levante-web` `healthy`.
2. **Portão D0.5** — backup + **restore testado** do `keyring` do Hiram **antes** de qualquer coisa
   pública (perder o volume `keyring` torna segredos de tenant indecifráveis). Nunca `docker compose down -v` em prod.
3. **Habilitar indexação:** `SITE_INDEXABLE=true` no `.env` → `docker compose up -d levante-web`
   (restart). `robots.txt` libera, `sitemap` sai, `X-Robots-Tag: noindex` some.
4. **Ativar newsletter:** `NEWSLETTER_ENABLED=true` (só com o `twilio-email` provisionado e um envio de teste
   comprovadamente entregue) → restart do `levante-web`. Ligar antes disso coleta inscrição que nunca
   receberia confirmação, porque o Mailpit captura e não entrega.
5. **Smoke:** `/api/health` + `/` + um GET dinâmico web→API (ver runbook). Conferir `https://felipemichel.com`
   e `https://www.felipemichel.com` (deve 301 para o apex).
6. Registrar `felipemichel.com` no **Google Search Console** + **Bing Webmaster**; submeter o `sitemap.xml`.
7. Escrever os artigos reais em `/admin/artigos/novo` (o banco de produção nasce vazio).

## Referências

- [ADR 0007](adr/0007-dominio-felipemichel-com.md) — a decisão do domínio.
- [runbook de lançamento](lancamento-runbook.md) — fluxo canônico, portões, segredos, rollback.
- `hiram/deploy/stack/README.md` — bring-up da stack, observabilidade, backups.
- `hiram/deploy/levante/README.md` — provisioning do tenant Levante.
