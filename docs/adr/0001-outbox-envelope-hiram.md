# ADR 0001 — Outbox transacional e contrato de eventos com o Hiram

Status: **Aceito (provisório no contrato)** · Fatia C1 · jul/2026

## Contexto

O Levante produz fatos de domínio (`ArtigoPublicado`, `ComentarioCriado`, `ComentarioAprovado` e, desde a Fatia C2, `AssinaturaSolicitada`, `AssinanteConfirmado`, `AssinaturaCancelada`) que o Hiram (projeto separado) precisa entregar como notificação (e-mail/push/WhatsApp). O site nunca chama provedor direto (CLAUDE.md, regra 7): publica eventos e o Hiram consome. Faltava o transporte.

**C2 (newsletter):** `AssinaturaSolicitada` carrega `email` + `token` no `dados` — é com eles que o Hiram monta o link de confirmação (`SITE_URL/newsletter/confirmar?token=…`). O e-mail é do próprio assinante (finalidade explícita: enviar a newsletter); nenhum outro dado pessoal entra no envelope.

## Decisões

1. **Mecanismo: Outbox transacional hand-rolled + Change Streams** (não Wolverine). O core do Wolverine virou MIT/grátis, então não é mais armadilha de licença — mas adotá-lo mudaria o estilo de handlers-diretos e o outbox dele com Mongo é território não-validado. O padrão nativo (Change Streams) é o que o blueprint chama de "mais robusto" para Mongo.

2. **Escrita atômica com degradação**: o agregado e seus eventos são gravados na **mesma transação** (`GravadorDeAgregadoMongo`). Transação exige replica set; onde não há (dev/test single-node), degrada para escrita sequencial best-effort (logada). Produção (Atlas) sempre é replica set.

3. **Relay como fila com reconciliação por polling**: o `outbox` é a fila. O relay (`RelayDeOutbox`, BackgroundService) varre a collection a cada poucos segundos, publica cada evento no RabbitMQ e **apaga** o doc. Entrega **at-least-once** — o consumidor deduplica por `eventId`. Optou-se por polling em vez de Change Streams: é robusto em qualquer topologia, é a rede de segurança correta contra perdas em failover (Change Streams podem pular eventos em failover/resharding), e a latência de segundos é irrelevante para notificação. Change Streams para latência menor ficam como otimização futura.

4. **Transporte**: RabbitMQ, exchange **topic durável** `levante.eventos`, **routing key = nome do evento** (o Hiram faz bind seletivo). Mensagens persistentes; `messageId = eventId`.

## Envelope (contrato PROVISÓRIO — o Felipe alinha o Hiram)

```json
{
  "eventId": "3f2504e0-4f89-41d3-9a0c-0305e82c3301",
  "tipo": "ArtigoPublicado",
  "versao": 1,
  "ocorridoEm": "2026-07-02T20:15:00.0000000Z",
  "dados": { "artigoId": "…", "slug": "…", "dataPublicacao": "…" }
}
```

- `tipo` = nome do fato (sem sufixo), também a routing key.
- `eventId` = idempotência no consumidor (o Hiram descarta repetidos).
- `dados` = payload do evento (JSON limpo; Guid como string).

**TODO (Felipe):** alinhar o Hiram a este envelope/exchange, ou me passar o formato que ele já consome para eu emitir naquele. Refinamento futuro: traduzir eventos de domínio em DTOs de integração curados (`...IntegrationEvent`) quando o contrato fechar, desacoplando o fio da forma do domínio.

## Consequências

- Confiabilidade: evento nunca é publicado sem o estado ter commitado, nem perdido após commit (com replica set). Sem replica set, best-effort (só dev/test).
- Operação: em produção o relay exige replica set (Atlas OK) e um broker RabbitMQ (Fase D provisiona host + credenciais no Key Vault). Se o relay não achar replica set/broker, loga erro e refaz com backoff (a fila acumula até normalizar).
- LGPD: `dados` carrega só o necessário (ids, slug); IP nunca entra (tratado na Fase B).
