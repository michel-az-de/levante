# ADR 0002 — Emissão de eventos para o Hiram via HTTP `POST /v1/events`

Status: **Aceito** · Fatia D (integração Hiram) · jul/2026 · **supera o transporte da [ADR 0001](0001-outbox-envelope-hiram.md)**

## Contexto

A ADR 0001 decidiu, em caráter **provisório**, publicar os eventos de integração num exchange RabbitMQ `levante.eventos` (routing key = nome do evento), deixando explícito o TODO: *"alinhar o Hiram a este envelope/exchange, ou me passar o formato que ele já consome para eu emitir naquele"* (GAP-I).

Ao fechar o GAP-I contra o código real do Hiram, descobriu-se que **o Hiram nunca consumiu esse exchange**. O Hiram é uma **plataforma multi-tenant de notificações** cujo ponto de integração congelado é o endpoint HTTP **`POST /v1/events`** (auth por `X-Api-Key`, corpo `SubmitEventRequest`), documentado em `hiram/docs/contracts/v1-events.md`. O Hiram tem o próprio outbox, o próprio RabbitMQ interno (`hiram.notifications`) e os próprios consumers/providers. Logo, o transporte da ADR 0001 estava **de-facto quebrado**: nada entregava as notificações.

Esta ADR troca o transporte para o contrato real do Hiram e resolve o GAP-I.

## Decisões

1. **Transporte: HTTP `POST /v1/events`** (não RabbitMQ). O Levante vira **tenant do Hiram**. O caminho RabbitMQ (`PublicadorRabbitMq`, exchange `levante.eventos`) é **aposentado**.

2. **Mantém-se o outbox transacional** da ADR 0001 (a garantia difícil — agregado + eventos na mesma transação via `GravadorDeAgregadoMongo`). Muda só o **destino do relay**: em vez de publicar no broker, faz `POST` no Hiram.

3. **Relay flag-based (não mais delete-on-publish).** O `RelayDeOutbox` lê os pendentes por `emissionSeq` crescente, faz o POST e **marca o documento** (`Enviada`/`Falhada`) em vez de apagar. Isso vira a **trilha de auditoria de emissão**. Nunca usar cursor `emissionSeq > last` (pularia um late-committer); ler sempre por `status == Pendente`.

4. **`emissionSeq` monotônico no Mongo.** Contrato do Hiram exige uma sequência monotônica por tenant (atributo de watermark). Como o Mongo não tem `bigserial`, uma coleção `sequencias` com `findOneAndUpdate($inc)` (dentro da transação do gravador) atribui o número.

5. **Cutover LEAN (sem shadow/parity/watermark).** O guia do Hiram (`easystok-emission-guide.md`) prevê shadow-mode e cutover por watermark porque o EasyStok **já enviava e-mail local** e não podia duplicar. O Levante **nunca enviou e-mail** (CLAUDE.md, regra 7) — não há entrega local para drenar nem paridade a validar. Vai direto para `live`. O `emissionSeq` fica só para ordenação/auditoria/idempotência.

6. **Cliente tipado fino + contract-test (deviação deliberada do guia).** O guia recomenda codegen do cliente a partir do OpenAPI do Hiram. Para 6 campos e 3 eventos, opta-se por um `HttpClient` tipado escrito à mão + um **contract-test contra um snapshot do sub-schema OpenAPI** do Hiram commitado no repo Levante (sem NuGet privado/submodule/acoplamento de build). O snapshot falha o teste se o contrato do Hiram derivar.

## Envelope (contrato congelado do Hiram — `POST /v1/events`)

O tenant vem da API key (`X-Api-Key`), não do corpo.

```json
{
  "eventType": "assinatura_solicitada",
  "eventId": "3f2504e0-4f89-41d3-9a0c-0305e82c3301",
  "emissionSeq": 42,
  "recipient": { "email": "assinante@exemplo.com" },
  "logicalAlertId": null,
  "timezone": null,
  "data": { "token": "…", "confirmUrlBase": "https://SITE_URL/newsletter/confirmar" }
}
```

- `eventType` = string canônica snake_case (o Hiram resolve canal/template via `Routine` por-tenant).
- `eventId` = idempotência (ver invariantes). É o `Id` do doc de outbox.
- `recipient.email` = contato no instante da emissão.
- `data` = variáveis do template (Scriban `StrictVariables` no Hiram → as chaves têm que bater exatamente com as vars do template).

## Mapa evento de domínio → notificação (v1)

| Evento de domínio | `eventType` | `recipient.email` | `data` |
|---|---|---|---|
| `AssinaturaSolicitada` | `assinatura_solicitada` | e-mail do assinante (do evento) | `token`, `confirmUrlBase` (`{SITE_URL}/newsletter/confirmar`) |
| `AssinanteConfirmado` | `assinante_confirmado` | e-mail do assinante (do evento) | `{}` |
| `ComentarioCriado` | `comentario_pendente` | admin (config `Levante:Notificacoes:AdminEmail`) | `comentarioId`, `artigoId`, `dataCriacao` |

`Tipo` de domínio sem mapeador (ex. `ArtigoPublicado`) → marca `Ignorada` (nunca `Enviada`; broadcast fica para fatia futura).

## Invariantes (não assumir o contrário)

- **Idempotência = `eventId` estável + índice único do Hiram.** O Hiram deduplica por `(tenant_id, event_id)` (`ux_events_tenant_event_id`) e, no replay, responde **`202` + header `Idempotency-Replayed: true`, NUNCA `409`**, sem novo fan-out. O guia falava "409(replay)" — impreciso. O cliente trata **202 (com/sem header) e 409 como sucesso**. A chave de dedupe é o `eventId` do **corpo** (o header `Idempotency-Key` a ingestão não lê; mantido como belt-and-suspenders). Garantia de exactly-once depende de o `eventId` ser **estável entre retentativas** = `Id` do outbox.
- **`emissionSeq` garante ordem de AUDITORIA, não de ENVIO.** Um late-committer com seq menor pode ser enviado **depois** de um seq maior já `Enviada`. Para os 3 eventos do v1 (independentes por assinante) é inócuo — mas **ninguém deve assumir FIFO de envio**.
- **Retry mora só no relay.** O `HiramClient` não faz retry (só `timeout` + `circuit breaker`); todo retry é do relay via `proximaTentativaEm` (backoff exponencial ou `Retry-After` do 429). Assim `tentativas` = nº real de POSTs (auditoria fiel).
- **`erroUltimaTentativa` é histórico ("último erro visto"), não "estado atual".** A auditoria lê `status` como verdade; o campo não é limpo ao virar `Enviada` nem no replay manual.
- **Relay single-replica.** O `levante-api` roda com 1 réplica (sem lease/lock no relay). Como a idempotência do Hiram está fechada, um POST concorrente do mesmo `eventId` é seguro; escalar réplicas exige lease antes (follow-up).

## Consequências

- Confiabilidade: evento nunca publicado sem o estado ter commitado (transação), nem perdido após commit; falha de rede/Hiram não perde (relay reprocessa por `proximaTentativaEm`). At-least-once com dedup no Hiram.
- Operação: em produção o relay exige replica set (transação) e o Hiram acessível. `Falhada` (teto de tentativas) é observável (métrica `emissoes_falhadas` + alerta) e reprocessável (query de replay manual: `status→Pendente, tentativas→0, proximaTentativaEm→null`).
- Segurança/LGPD: `data` carrega só o necessário (token, ids, slug); IP nunca entra; a `Hiram:ApiKey` vem de user-secrets/Key Vault, nunca do repo.
- Observabilidade: OTel no Levante (traces/métricas/logs) → OTLP; `traceparent` é injetado automaticamente no POST (o Hiram lê `Activity.Current.Id`), formando um trace único Levante→Hiram→provider.
