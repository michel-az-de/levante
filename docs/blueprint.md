# Plano E2E, Site Pessoal / Portfólio / Plataforma de Conteúdo

Stack base: .NET (LTS atual, 9 ou 10), Blazor Web App + Minimal API, Clean Architecture, DDD, SOLID, MongoDB, integração com Hiram para notificações.

Objetivo estratégico: ser o **centralizador oficial** da sua atuação em tecnologia (tech lead, arquiteto, gestor, dev senior/full stack), canal de autoridade, captação de freelas e prova viva de arquitetura (o próprio site é peça de portfólio).

---

## 0. Sumário executivo + onde eu discordo / preciso de decisão

| # | Decisão | Minha recomendação | Por que importa |
|---|---------|--------------------|-----------------|
| 1 | Arquitetura macro | **Monólito modular** com Clean Arch por módulo, não microserviços | Microserviço aqui é over-engineering. Um monólito modular com fronteiras limpas já demonstra maturidade arquitetural e permite extrair serviço depois sem pagar custo operacional agora. Vende mais "arquiteto sênior" do que 8 serviços para um blog. |
| 2 | "Assinar documentos" | Decidir o **nível** (ver §11). MVP nível 1-2, jurídico só se necessário | Assinatura com validade jurídica (ICP-Brasil/PAdES) é projeto à parte, com certificado e carimbo de tempo. "Assinar" pode significar 4 coisas muito diferentes. |
| 3 | "Modelo científico" | Decidir profundidade: template visual vs **citabilidade real** (DOI/ORCID) | DOI via Zenodo é grátis e integra com seu GitHub. Isso é o que de fato dá autoridade de "fonte oficial". |
| 4 | MongoDB | Atlas gerenciado, **replica set obrigatório** | Você é Postgres por padrão. Mongo serve bem a conteúdo, mas transações e Change Streams (necessários para o outbox do Hiram) exigem replica set. Sem isso, o outbox vira gambiarra. |
| 5 | LGPD | **Não é opcional** | "Ver de onde acessou" = IP/geo = dado pessoal. Banner de consentimento + política + retenção são obrigatórios, não enfeite. |
| 6 | Escopo | Sequenciar em fatias, walking skeleton primeiro | A lista é grande. Tentar tudo de uma vez trava. Site indexável que "te vende" sai já na Fatia 1. |

GAPs detalhados que dependem de você estão em §15.

---

## 1. Visão e posicionamento

O site precisa entregar 3 trabalhos ao mesmo tempo:

1. **Autoridade técnica**, artigos e publicações que provam profundidade (arquitetura .NET, DDD, Azure, microserviços).
2. **Vitrine comercial**, portfólio + serviços + CTA de freela, com prova social.
3. **Hub de identidade**, fonte canônica de "quem é o Felipe em tecnologia", referenciada por LinkedIn/GitHub.

Página "Sobre" e perfil são tratados como **entidade Person** (schema.org), com `sameAs` apontando para LinkedIn e GitHub. Isso é o que habilita você a aparecer com autoridade nas buscas (elegibilidade a knowledge panel).

Formato de portfólio recomendado (vende arquiteto): cada projeto como **case study** = Problema → Arquitetura → Decisões (trade-offs) → Resultado com métricas. Não lista de tecnologias, e sim narrativa de decisão.

Meta-portfólio: documentar a arquitetura do próprio site numa página `/arquitetura` (ou publicar os ADRs). O site provando-se a si mesmo é o melhor case.

---

## 2. Mapa de capacidades (escopo E2E)

| Capacidade | Resumo |
|------------|--------|
| Publicação de conteúdo | Artigos, notícias, publicações científicas, rascunhos, versões, taxonomia |
| Engajamento | Curtir, compartilhar, comentar (com fila de aprovação) |
| Audiência | Newsletter (double opt-in), leads, consentimentos |
| Telemetria | Quem/quando/de onde acessou, dashboard, origem geográfica |
| SEO | SSR, structured data, sitemap, Core Web Vitals, RSS |
| Documentos | Geração PDF/Word, assinatura, template científico |
| Portfólio | Projetos, skills, depoimentos, integração GitHub |
| Leads/WhatsApp | Captação, lifecycle, click-to-chat |
| Notificações/Lembretes | Eventos publicados para o **Hiram** entregar (email/push/whatsapp) |
| Admin/Segurança | Login + MFA, moderação, hardening, LGPD |

---

## 3. Bounded contexts (DDD)

Monólito modular: cada contexto é um módulo independente com Clean Arch própria (Domain / Application / Infrastructure). Comunicação interna por mensagens in-process; comunicação com Hiram por **eventos de integração**.

| Contexto | Responsabilidade | Agregados |
|----------|------------------|-----------|
| **Content** | Artigos, notícias, publicações, versões, slug, taxonomia | `Article`, `Publication`, `Category`, `Tag` |
| **Engagement** | Curtidas, comentários (workflow), compartilhamentos | `Comment`, `Reaction`, `Share` |
| **Audience** | Leads, assinantes, consentimentos | `Lead`, `Subscriber`, `Consent` |
| **Analytics** | Visitas, sessões, geo, referrers | `VisitEvent` (time-series), `Session` |
| **Identity** | Auth admin, roles, MFA | `AdminUser`, `Role` |
| **Documents** | Geração PDF/Word, assinatura | `Document`, `SignatureRequest` |
| **Portfolio** | Projetos, skills, depoimentos, GitHub | `Project`, `Skill`, `Testimonial` |
| **Integração (saída)** | Outbox + publicação de eventos para Hiram | Integration Events (não é contexto com estado de domínio) |

Regra de ouro: nenhum módulo chama provedor de email/push direto. Tudo vira **evento** que o Hiram consome. O site sabe *o que* aconteceu, o Hiram sabe *como* entregar.

---

## 4. Arquitetura de solução

Camadas (Clean Architecture, aplicadas por módulo):

- **Domain**: entidades, value objects, agregados, domain events, specifications. Zero dependência de infra.
- **Application**: casos de uso (Commands/Queries, CQRS-lite), validators, ports (interfaces). Orquestra, não conhece Mongo.
- **Infrastructure**: repositórios MongoDB, cliente GitHub, cliente WhatsApp, relay do outbox, publisher para Hiram, GeoIP.
- **Presentation**: Blazor Web App (público SSR + admin interativo) + Minimal API (webhooks, ingestão de analytics, RSS/sitemap, API headless).

Fluxo de notificação (núcleo da integração com Hiram):

```
Caso de uso grava estado + evento no Mongo (mesma transação)
        ↓ (Change Stream observa a collection outbox)
Relay publica no RabbitMQ
        ↓
Hiram consome e entrega (email / push / whatsapp)
```

Idempotência por `eventId`, retry e DLQ no consumidor. Esse é o mesmo padrão de outbox que você já fez no Hiram, agora com Change Streams do Mongo no lugar de polling.

---

## 5. Stack técnica + ADRs principais

| Camada | Escolha | Nota / ADR |
|--------|---------|------------|
| Runtime | .NET LTS atual (9/10) | Confirmar versão LTS antes de fixar |
| Frontend | **Blazor Web App**, Static SSR para conteúdo público + render interativo (Server/WASM) para admin e widgets | SSR é inegociável para SEO. Mantém tudo em C#. Alternativa mais simples: Razor Pages no público + Blazor no admin (GAP-D) |
| API | **Minimal API** por feature (endpoints agrupados) | Webhooks (GitHub/WhatsApp), ingestão de analytics, RSS, sitemap |
| Mediator/Mensageria | MediatR (**checar licenciamento atual, mudou recentemente**) vs **Wolverine** vs hand-rolled | Wolverine unifica mediator + outbox + RabbitMQ, mas o outbox dele assume persistência própria; com Mongo precisa validar. Padrão Mongo nativo (Change Streams) é o mais robusto (GAP-F) |
| Persistência | **MongoDB.Driver** (nativo) + abstração de repositório | Provider EF Core p/ Mongo existe, mas para agregados ricos o driver nativo dá controle total |
| Banco | **MongoDB Atlas**, replica set | Necessário para transações + Change Streams + Atlas Search (full-text) |
| Validação | FluentValidation | No pipeline de Application |
| Logs/Tracing | Serilog + OpenTelemetry → otel-lgtm na VM | Estruturado desde o dia 1; App Insights só se voltar ao Azure |
| PDF | **QuestPDF** (licença Community grátis abaixo do limite de receita) | §11 |
| Word | OpenXML SDK (skill docx) | §11 |

---

## 6. Modelo de dados (MongoDB)

| Collection | Tipo | Observações |
|------------|------|-------------|
| `articles` | Documento | Conteúdo rico, versões embutidas, metadados SEO por artigo, slug único indexado |
| `publications` | Documento | Modelo científico (autores, abstract, referências, DOI) |
| `categories` / `tags` | Documento | Taxonomia |
| `comments` | Documento | Referência ao artigo + estado (`pending`/`approved`/`spam`) |
| `reactions` | Documento | Curtidas/compartilhamentos agregados |
| `subscribers` | Documento | Newsletter, estado de opt-in, token de confirmação |
| `leads` | Documento | Origem/UTM, lifecycle, score |
| `consents` | Documento | Registro LGPD (base legal, timestamp, escopo) |
| `visit_events` | **Time-series** | Path, referrer, UTM, geo, device, sessão. TTL/retention configurável |
| `outbox` | Documento | Eventos de integração pendentes (Change Stream observa esta) |
| `admin_users` | Documento | Identity + MFA |

Pontos de atenção (são onde Mongo "morde"):
- **Transações** só com replica set. Outbox depende disso (gravar estado + evento atomicamente).
- **Analytics** = pipeline de agregação. Modele `visit_events` como time-series desde o início, refazer depois é caro.
- **Full-text** de artigos via Atlas Search (ou text index).
- **Slug** com índice único; evita duplicidade e ajuda canonical/SEO.

---

## 7. Design system + frontend

- **Tokens** (cores, tipografia, espaçamento, raios, sombras, motion) como fonte única, versionados. Light/dark.
- **Biblioteca de componentes** Blazor reutilizáveis (card de artigo, hero, CTA, formulário de lead, badge de publicação, blocos de case study).
- **Acessibilidade** (WCAG AA): contraste, navegação por teclado, semântica. Acessibilidade também é sinal de SEO/qualidade.
- **Performance como parte do design**: imagens AVIF/WebP com dimensões definidas, fontes com `preload`, CSS crítico, lazy load. Isso protege os Core Web Vitals.
- Identidade visual coerente com sua marca pessoal (consistente com LinkedIn/GitHub).

---

## 8. SEO (estratégia + aplicação técnica)

### Técnico (no código)
- **Render**: todo conteúdo público em Static SSR (HTML pronto no servidor, sem depender de JS para indexar).
- **URLs**: slugs limpos kebab-case, sem IDs; `canonical` em toda página; redirects 301 ao mudar slug.
- **Sitemap.xml dinâmico** gerado dos artigos publicados + `robots.txt`.
- **Core Web Vitals**: LCP < 2,5s, INP < 200ms, CLS < 0,1.
- **Structured data (JSON-LD)**:
  - `Person` no perfil com `sameAs` (LinkedIn, GitHub), `jobTitle`, `knowsAbout`. **Este é o ativo de marca pessoal.**
  - `BlogPosting`/`Article` por artigo; `ScholarlyArticle` para publicações científicas.
  - `BreadcrumbList`, `WebSite` + `SearchAction`.
- **Meta por artigo** (editável no admin): title ≤60 char, description ≤155, Open Graph, Twitter Card, `article:published_time`, autor.
- **RSS/Atom feed**.
- **hreflang** se houver PT + EN (GAP-H).
- 404 custom, HTTPS, HSTS.
- Integração com **Google Search Console** + envio de sitemap; Bing Webmaster.

### Conteúdo / off-page
- Estratégia de palavras-chave por **pilar** (arquitetura .NET, DDD, Azure, microserviços, liderança técnica).
- Calendário editorial.
- **Cross-post com canonical**: ao republicar no LinkedIn/Medium/dev.to, usar `rel=canonical` apontando para o seu site. Assim você não compete consigo mesmo e o site fica como fonte de autoridade.
- Linkagem interna + "artigos relacionados".

---

## 9. Analytics + LGPD

### Coleta (first-party, dados seus)
- Captura: timestamp, path, referrer, UTM, user-agent (→ device/browser/OS), IP → geo (país/cidade via MaxMind GeoLite2), sessão (cookie first-party), duração.
- **Anonimização**: após resolver geo, hashear ou truncar o IP. Configurável.
- Armazenamento em `visit_events` (time-series), com retenção definida.
- Dashboard admin: visitas no tempo, top conteúdo, origem geográfica (mapa), referrers, dispositivos, funil de leads.
- Complementar com Search Console para *queries* de busca reais.

### LGPD (obrigatório)
- Banner de **consentimento** (cookies/analytics) antes de rastrear.
- Página de **política de privacidade**, base legal definida (legítimo interesse vs consentimento), retenção, contato.
- **Registro de consentimento** (collection `consents`), opt-out, direito de exclusão.

---

## 10. Integrações

### GitHub
- Exibir repos fixados, linguagens, stars, último commit, gráfico de contribuição (GraphQL `contributionsCollection`).
- Opcional: importar README como página de projeto.
- **Cache** (a API tem rate limit); webhook de `push` invalida cache.
- Token PAT fine-grained read-only ou GitHub App.
- Vincular projetos do portfólio aos repos.

### Leads + WhatsApp
- Captação: formulários (contato, orçamento de freela, newsletter) com origem/UTM → `leads`.
- **Inbound WhatsApp (MVP)**: link `wa.me` / click-to-chat com mensagem pré-preenchida. Zero custo.
- **Outbound/automação (depois)**: WhatsApp Business Cloud API (Meta), exige conta Business + templates aprovados + custo por conversa.
- Lifecycle: novo → contatado → qualificado → ganho/perdido; score por origem.
- Novo lead dispara notificação **via Hiram** (email/whatsapp/push pra você).
- Recomendação: começar com click-to-chat + form + notificação Hiram. Cloud API só se quiser automação real (GAP-G).

### Newsletter + Lembretes
- Assinatura com **double opt-in** (LGPD + entregabilidade): form → email de confirmação → confirmado.
- Listas/segmentos, templates, unsubscribe one-click.
- Envio e **lembretes/agendamentos** sempre via Hiram: o site publica evento (`ArtigoPublicado`, `CampanhaNewsletter`, `LembreteAgendado`), o Hiram entrega.
- Métricas de abertura/clique se o Hiram suportar pixel/links rastreáveis.

### Hiram (contrato de integração)
- Site = produtor; Hiram = entrega.
- Eventos versionados sugeridos: `ArtigoPublicado`, `ComentarioAprovado`, `NovoLead`, `AssinanteConfirmado`, `CampanhaNewsletter`, `LembreteAgendado`.
- Transporte: Outbox (Mongo) → Change Stream → RabbitMQ → Hiram.
- Definir junto: o Hiram reusa schema existente? Como ele consome hoje? (GAP-I)

---

## 11. Assinatura de documentos (decidir o nível)

| Nível | O que é | Validade | Esforço | Ferramentas |
|-------|---------|----------|---------|-------------|
| 1. Geração | PDF/Word a partir de template/conteúdo | n/a | Baixo | QuestPDF, OpenXML |
| 2. Assinatura visual | Imagem de assinatura + metadados no doc | Não jurídica | Baixo-médio | QuestPDF/OpenXML |
| 3. Assinatura digital (PAdES) | Certificado X.509, PKCS#7, carimbo de tempo opcional | Média-alta | Alto | iText (licença) / BouncyCastle + certificado A1 |
| 4. ICP-Brasil | Certificado ICP-Brasil A1/A3 + carimbo credenciado | Jurídica plena (BR) | Muito alto | ICP-Brasil, ITI, token A3/HSM |

Recomendação: **MVP nível 1-2** (PDFs autorais com aparência oficial). Para "autoridade científica", **nível 3 + DOI** já é mais que suficiente. **Nível 4 (ICP-Brasil)** só se você for assinar contrato de freela com valor jurídico pelo site, é projeto à parte. (GAP-B)

---

## 12. Modelo científico de publicação

- **Template**: título, autores + afiliação, abstract, palavras-chave, corpo estruturado (seções), referências (ABNT ou APA), identificador.
- **Citabilidade real (recomendado)**:
  - **ORCID** (ID de pesquisador).
  - **DOI via Zenodo** (grátis, integra direto com releases do GitHub). É o caminho mais simples para suas publicações virarem citáveis de verdade.
- Versionamento de publicações (v1, v2) com histórico.
- Export PDF acadêmico (via contexto Documents).
- Badge "Cite this work" gerando **BibTeX/RIS**.
- Métricas: downloads, citações.

Isso é o que transforma "blog" em "fonte oficial". (GAP-C define a profundidade.)

---

## 13. Segurança, admin, infra/DevOps

### Admin + segurança
- Auth: ASP.NET Core Identity + **TOTP MFA** (admin único). Alternativa: Entra ID externo. Roles: Admin (futuro: Editor).
- **Moderação**: fila de comentários pendentes → aprovar/reprovar/spam; anti-spam (honeypot + rate limit + heurística).
- Headers: HSTS, CSP, X-Content-Type-Options, Referrer-Policy; anti-forgery; rate limiting nativo do .NET; validação de entrada.
- Secrets: **`.env` na VM (chmod 600, backup off-host cifrado)** (prod) / User Secrets (dev). Key Vault fica como hardening se voltar ao Azure (ADR 0003).

### Infra
- Host: **VM conjunta com o Hiram via Docker Compose**, Caddy como borda pública (GAP-J resolvido, ver `docs/adr/0003-hospedagem-vm-conjunta-hiram.md`). A análise original comparava Container Apps vs App Service; a VM ganhou por custo e por encaixar o relay always-on.
- DB: **MongoDB Atlas** (replica set, backup gerenciado, Atlas Search), externo à VM com usuário de privilégio mínimo.
- **Backup/DR como item de primeira classe**: backup automático testado com restore periódico. Backup que nunca foi restaurado não é backup.
- CI/CD: **GitHub Actions** (fecha o ciclo com a integração GitHub) → build/test, publica imagens no GHCR, CD escopado por SSH na VM.
- IaC/config: stack **Docker Compose** + scripts de provisionamento (repo Hiram, `deploy/stack`). Bicep/Terraform ficaram fora com a VM (ADR 0003).
- CDN: Azure Front Door para cache de borda e assets (ajuda CWV/SEO).

---

## 14. Roadmap em fatias (vertical slices)

| Fatia | Entrega | Valor ao final |
|-------|---------|----------------|
| **0. Walking skeleton** | Blazor Web App SSR + Mongo conectado + 1 página + CI/CD + healthcheck | Esteira fim a fim provada |
| **1. Conteúdo público + SEO base** | Listar/ler artigos (seed), slugs, sitemap, JSON-LD Person+Article, OG, RSS, CWV | **Site já indexável e "te vende"** |
| **2. Admin + publicação** | Login + MFA, editor de artigo, publicar/despublicar, taxonomia, meta SEO editável | Você publica sozinho |
| **3. Engajamento** | Curtir, compartilhar, comentar com fila de aprovação + anti-spam | Interação pública |
| **4. Audiência + Hiram** | Newsletter double opt-in, outbox → RabbitMQ → Hiram, `ArtigoPublicado` dispara envio | Notificações reais |
| **5. Leads + WhatsApp** | Form de lead, wa.me, notificação de lead via Hiram, mini-CRM | Canal de freela ativo |
| **6. Analytics + LGPD** | Coleta first-party, geo, dashboard, banner de consentimento, política | "Quem/quando/de onde" |
| **7. Portfólio + GitHub** | Projetos, skills, depoimentos, integração GitHub | Vitrine de arquiteto |
| **8. Documentos + científico** | Geração PDF/Word, template científico, assinatura (nível decidido), DOI/ORCID | Fonte oficial citável |
| **9. Polish** | Design system completo, performance, i18n PT/EN, hardening | Acabamento |

Princípio: cada fatia é vertical (entrega valor de ponta a ponta), não horizontal (não "fazer todo o backend primeiro"). Walking skeleton antes de qualquer feature, igual você fez no Hiram.

---

## 15. Portões de decisão (GAPs)

| GAP | Pergunta | Recomendação |
|-----|----------|--------------|
| **A** | Nome e domínio do site, identidade de marca | Definir cedo (afeta SEO, schema, branding) |
| **B** | Nível de assinatura de documentos (1-4) | MVP 1-2; jurídico só se necessário |
| **C** | Profundidade do modelo científico | Template + DOI via Zenodo (grátis, integra GitHub) |
| **D** | Frontend: Blazor Web App vs Razor Pages público + Blazor admin | Blazor Web App |
| **E** | MongoDB Atlas vs self-host | Atlas (replica set p/ transações + Change Streams) |
| **F** | Mediator/mensageria: MediatR / Wolverine / hand-rolled | Validar licença MediatR; avaliar Wolverine; outbox Mongo nativo via Change Streams |
| **G** | WhatsApp: click-to-chat (MVP) vs Cloud API | Click-to-chat agora, Cloud API só com automação |
| **H** | Idiomas: PT só ou PT + EN | PT+EN amplia alcance mas dobra esforço de conteúdo e exige hreflang |
| **I** | Contrato de eventos com Hiram | Reusar schema existente; alinhar como o Hiram consome hoje |
| **J** | Hospedagem: Container Apps vs App Service vs VM | **VM conjunta com o Hiram (ADR 0003)** |
