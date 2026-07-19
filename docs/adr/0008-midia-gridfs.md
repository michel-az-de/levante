# ADR 0008 — Armazenamento de mídia de artigo: GridFS no Mongo do contexto

Status: **Aceito** · Fatia de autoria rica de artigos (1/3) · jul/2026

## Contexto

Até aqui o corpo do artigo era markdown puro e imagem só entrava por **URL externa digitada à mão**:
não havia upload, storage, nem qualquer rota de mídia em toda a stack. A fatia de autoria rica
(editor WYSIWYG + importação de HTML/URL) exige receber arquivo do admin e servi-lo publicamente.

Restrições que moldaram a decisão:

- **Volume real é pequeno.** Blog pessoal, um único autor, imagens de artigo técnico. Ordem de
  grandeza: dezenas de arquivos por ano, ≤5MB cada.
- **A produção roda numa VM compartilhada com o Hiram** via Docker Compose ([ADR 0003](0003-hospedagem-vm-conjunta-hiram.md)),
  com **Mongo self-hosted** (o Atlas previsto no ADR 0003 não sobreviveu ao guardrail de privilégio
  mínimo no cutover). Disco é recurso compartilhado entre os dois produtos.
- **O repositório é peça de portfólio**: o padrão de engenharia importa tanto quanto a feature, então
  "sobe num container e serve estático" não passa.

## Alternativas consideradas

| Opção | A favor | Contra |
|-------|---------|--------|
| **GridFS no Mongo do contexto** (escolhida) | Zero serviço novo, zero credencial nova, zero custo; entra no mesmo backup do banco; `MongoDB.Driver` já é dependência (GridFS vem no pacote desde a v3, sem pacote extra) | Binário no mesmo banco/disco do dado transacional; `mongodump` engorda; sem CDN nativo |
| **Azure Blob Storage** | Escala, CDN e lifecycle prontos; tira binário do banco | Recurso novo na Azure, credencial nova para gerir e rotacionar, custo recorrente, e mais uma peça no cutover — desproporcional ao volume |
| **Volume em disco na VM** | O mais simples de todos | Backup separado do banco (o portão D0.5 já cobre volume, mas seria mais um), e some num recreate de container mal feito |

## Decisões

1. **Mídia vai para GridFS**, no mesmo Mongo do contexto Conteudo, bucket `midias`
   (`midias.files` / `midias.chunks` são collections normais e entram no `mongodump` junto).

2. **Sem agregado de domínio.** Mídia não tem invariante de negócio: é blob + metadados. O `Domain`
   recebe apenas a porta `IMidiaStorage` (DIP — a abstração mora na camada de dentro); o GridFS fica
   confinado na `Infrastructure`, como manda o princípio 6 do CLAUDE.md.

3. **Id da mídia é `Guid`** (`GridFSBucket<Guid>`), não `ObjectId`, para casar com a rota pública
   `/midias/{id:guid}`. Isso exigiu registrar o `GuidSerializer(Standard)` globalmente, porque o
   bucket não tem um `Document` próprio onde pendurar `[BsonGuidRepresentation]` como os demais.

4. **URL persistida no markdown é sempre relativa** (`/midias/{id}`), servida pelo BFF do Next na
   mesma origem. Host absoluto no corpo do artigo seria dívida permanente (mudança de domínio exigiria
   reescrever conteúdo) e forçaria mexer na CSP.

5. **Leitura é pública e cacheada de forma agressiva** (`Cache-Control: public, max-age=31536000,
   immutable` + ETag): o id nunca é reusado, então o conteúdo daquela URL é imutável por construção.

6. **Escrita valida em profundidade**: autorização JWT, limite de 5MB cortado no transporte,
   allowlist de content-type **e** conferência da assinatura binária (magic bytes) — o header do
   cliente sozinho não é evidência do que o arquivo é.

## Consequências

**Aceitas nesta fatia:**

- **Mídia órfã existe e não é coletada.** Rascunho abandonado ou imagem trocada deixa blob para trás,
  e apagar um artigo não apaga suas imagens. Aceito para a v1 por não haver ainda vínculo
  artigo↔mídia; rastreado em issue própria, a ser resolvido junto com a fatia do editor — que é
  quem torna o problema real (colar imagem = upload por colagem).
- **Exclusão não recolhe cache.** `DELETE /admin/midias/{id}` remove do banco, mas quem já baixou
  continua com a cópia até o cache expirar. É inerente a cache imutável, não a esta implementação.

**A vigiar (o custo real da escolha):**

- **Disco do Mongo vira recurso crítico compartilhado.** Como a VM hospeda Levante e Hiram, encher o
  disco derruba os dois. O volume cresce por design nas próximas fatias (editor sobe a cada colagem;
  importação baixa todas as imagens da página). Ver a nota de operação em
  [`lancamento-runbook.md`](../lancamento-runbook.md).
- **A janela de backup/restore cresce com os binários**, porque agora o `mongodump` os arrasta junto.

**Gatilho de revisão** — sair do GridFS para blob/CDN se qualquer um acontecer:
o bucket passar de ~2GB, o `mongodump` deixar de caber na janela do portão D0.5, ou o blog passar a
servir imagem em volume que justifique CDN.
