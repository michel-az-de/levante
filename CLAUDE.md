# CLAUDE.md, Levante

Instruções operacionais para o Claude Code neste repositório. Leia antes de planejar qualquer tarefa.

## Visão geral

Levante é a plataforma pessoal e portfólio técnico do Felipe (arquiteto de soluções e dev sênior .NET / full stack): blog técnico, publicações, vitrine de projetos e hub de identidade. O próprio repositório é peça de portfólio, então o padrão de engenharia importa tanto quanto a feature.

## Fonte de verdade

A pasta `/docs` contém o blueprint, o mapa técnico, a convenção de nomes completa e os ADRs. Leia esses arquivos antes de propor um plano. Em caso de conflito, `/docs` e este `CLAUDE.md` vencem sobre suposições. Não invente decisão que contrarie esses documentos.

## Stack (travada)

| Camada | Tecnologia |
|--------|-----------|
| Backend | .NET (LTS) · Minimal API · Clean Architecture · DDD · SOLID · CQRS-lite |
| Banco | MongoDB Atlas · driver nativo (`MongoDB.Driver`) · Repository por agregado |
| Frontend | Next.js (SSR/SSG) · React · TypeScript · Tailwind + shadcn/ui |
| Contrato front↔back | OpenAPI: o front consome tipos gerados, não escreve DTO à mão |
| Mensageria | Outbox (Mongo) → Change Streams → RabbitMQ |
| Notificações | Responsabilidade de outro projeto (Hiram). Aqui só publicamos eventos |
| Validação | FluentValidation (pipeline) + guard clauses (domínio) |
| Logs/Tracing | Serilog + OpenTelemetry |

## Princípios de arquitetura

1. Monólito modular. Cada bounded context é um módulo com Clean Architecture própria.
2. Dependências apontam para dentro. `Domain` não conhece infraestrutura nem framework.
3. Camadas por contexto: `.Domain` (entidades, VOs, eventos, specifications), `.Application` (Commands/Queries, handlers, ports, validators), `.Infrastructure` (repositórios Mongo, clientes externos, relay do outbox).
4. CQRS-lite: Command/Query + Handler. Sem lib de mediator por enquanto (ver GAPs); o handler é chamado direto.
5. Result pattern na Application. Exception só para falha excepcional ou de infraestrutura, nunca para fluxo de negócio esperado.
6. Repository por agregado. O `MongoDB.Driver` fica encapsulado na `Infrastructure`, nunca vaza para Application ou Domain.
7. O site nunca chama provedor de e-mail/push direto. Toda notificação vira evento de integração gravado no Outbox.

## Bounded contexts

`Conteudo`, `Engajamento`, `Audiencia`, `Analytics`, `Identity`, `Documents`, `Portfolio`, mais a borda de `Integracao` (outbox). Namespaces no formato `Levante.<Contexto>.<Camada>`.

## Convenção de nomes (regra mestra)

`[ConceitoDeDomínioPT][PadrãoTécnicoEN]`. O pedaço em PT-BR nomeia o que a coisa é no negócio (linguagem ubíqua); o pedaço em EN nomeia o papel técnico. Dentro do modelo de domínio (entidade, VO, evento, comportamento) tudo é PT. A casca técnica que embrulha o domínio é EN.

| Artefato | Convenção | Exemplo |
|----------|-----------|---------|
| Entidade / Agregado | domínio PT | `Cliente`, `Artigo`, `Comentario` |
| Value Object | domínio PT | `Email`, `Slug`, `Cpf` |
| Enum (tipo e valores) | domínio PT | `StatusComentario { Pendente, Aprovado, Rejeitado }` |
| Propriedade | domínio PT | `cliente.Nome`, `comentario.DataCriacao` |
| Comportamento (método de domínio) | verbo PT | `comentario.Aprovar()`, `comentario.MarcarComoSpam()` |
| Domain Service | capacidade PT | `Precificador`, `AvaliadorDeLead` |
| Domain Event | fato PT, sem sufixo | `ComentarioAprovado`, `ArtigoPublicado` |
| Integration Event | PT + sufixo EN | `ArtigoPublicadoIntegrationEvent` |
| Command | verbo+noun PT + `Command` | `CriarClienteCommand`, `AprovarComentarioCommand` |
| Command Handler | `...CommandHandler` | `CriarClienteCommandHandler` |
| Query | `Obter/Listar...` PT + `Query` | `ObterArtigoPorSlugQuery` |
| Repository (interface e classe) | noun PT + `Repository` | `IClienteRepository`, `ClienteRepository` |
| Métodos de Repository | EN (classe técnica) | `GetByIdAsync`, `AddAsync`, `ListAsync` |
| Consumer (mensageria) | PT + `Consumer` | `ArtigoPublicadoConsumer` |
| DTO / Request / Response | PT + EN | `ClienteDto`, `CriarClienteRequest`, `ArtigoResponse` |
| Validator | PT + `Validator` | `CriarClienteCommandValidator` |
| Endpoints (Minimal API) | noun PT + `Endpoints` | `ArtigoEndpoints` |
| Domain Exception | frase PT + `Exception` | `ClienteNaoEncontradoException` |
| Specification | PT + `Specification` | `ClienteAtivoSpecification` |
| Namespace de contexto | domínio PT | `Levante.Conteudo` |
| Namespace de camada | técnico EN | `.Domain`, `.Application`, `.Infrastructure` |

Persistência (MongoDB): collection em PT minúsculo plural (`artigos`, `comentarios`); campos do documento em PT camelCase (`dataCriacao`, `valorTotal`); collection técnica em EN (`outbox`).

Frontend (Next.js/TS): tipo de domínio em PT (gerado do OpenAPI); componente de domínio = noun PT + papel EN (`ArtigoCard`, `ComentarioForm`); hook = `use` + PT (`useArtigos`); rota com slug PT (`/artigos/[slug]`); componente genérico de UI sem domínio em EN (`Button`, `Modal`, vindos do shadcn).

Sub-regras obrigatórias:
1. Identificadores em código: PT sem acento e sem cedilha (`Conteudo`, `Comentario`, `Endereco`, `Publicacao`). Acento e cedilha só em strings de UI, conteúdo e dados.
2. Casing: PascalCase em tipos/membros C#, camelCase em campos Mongo e em TS.
3. Auditoria no domínio em PT (`DataCriacao`, `DataAtualizacao`). Mensagens e códigos de erro (Result) em PT, voltados ao usuário.

Tabela completa e exemplos de um slice inteiro estão em `/docs/convencao-de-nomes.md`.

## Segurança (não-negociável)

Estas regras vêm de incidentes reais. Não relaxe nenhuma.

1. Nenhum secret no repositório. Configuração via user-secrets (dev) e variáveis de ambiente / Azure Key Vault (prod).
2. A conta de runtime do MongoDB usa privilégio mínimo (read/write na database, sem role administrativa). Deve existir um teste que falha se a conta de runtime tiver privilégio administrativo. Isso roda no gate `polish`.
3. Todo endpoint nasce com autorização explícita. Nada de endpoint sem política definida.
4. Headers de segurança (HSTS, CSP, etc.), anti-forgery, rate limiting e validação de entrada são padrão, não opcional.
5. LGPD: coleta de analytics exige consentimento; IP é anonimizado; há política de retenção. Sem rastrear antes do consentimento.
6. Admin (contexto `Identity`): login com senha (PasswordHasher/PBKDF2) + lockout (5 falhas/15min) e rate limit no `/auth/login`; **JWT bearer** (sem enumeração de usuário). `Jwt:SecretKey` e o admin de seed (`Admin:Email`/`Admin:SenhaInicial`) vêm de user-secrets/env, nunca do repo. MFA (TOTP) é hardening posterior.

## Estilo de código C#

1. C# moderno: file-scoped namespaces, primary constructors, `record` para VOs e DTOs, collection expressions.
2. `Nullable` habilitado e `TreatWarningsAsErrors` ligado. Warning é build quebrado, não dívida.
3. `var` quando o tipo é óbvio; tipo explícito quando ajuda a leitura.
4. Guard clauses no domínio para invariantes; FluentValidation no pipeline para entrada.
5. `async`/`await` com sufixo `Async` em métodos assíncronos; `CancellationToken` propagado.

## Estilo frontend

1. SSR/SSG para todo conteúdo público (SEO). Sem SPA puro.
2. Tipos da API sempre gerados do OpenAPI. Nunca duplicar contrato à mão.
3. Tailwind + shadcn/ui; componentes de domínio no padrão de nomes acima.

## Estrutura de pastas

```
levante/
├─ .github/workflows/ci.yml          # gates: rough-cut → dress → polish → raise
├─ .github/dependabot.yml
├─ .github/CODEOWNERS
├─ docs/                             # blueprint, mapa, convenção, ADRs (fonte de verdade)
├─ src/
│  ├─ api/
│  │  ├─ Levante.sln
│  │  ├─ host/Levante.Api/                       # host Minimal API
│  │  ├─ shared/Levante.SharedKernel/
│  │  └─ contexts/Conteudo/
│  │     ├─ Levante.Conteudo.Domain/
│  │     ├─ Levante.Conteudo.Application/
│  │     └─ Levante.Conteudo.Infrastructure/
│  └─ web/                            # Next.js
├─ tests/
│  ├─ Levante.ArchitectureTests/
│  ├─ Levante.Conteudo.UnitTests/
│  └─ Levante.Api.IntegrationTests/
├─ Directory.Build.props  Directory.Packages.props  global.json
├─ .editorconfig  .gitattributes  .gitignore
├─ CLAUDE.md  README.md  SECURITY.md  LICENSE
```

## Comandos

| Ação | Comando |
|------|---------|
| Build backend | `dotnet build src/api/Levante.sln` |
| Testes (unit, sem Docker) | `dotnet test src/api/Levante.sln --filter "Category!=Integration"` |
| Testes (integração, requer Docker) | `dotnet test src/api/Levante.sln --filter "Category=Integration"` |
| Format (check) | `dotnet format src/api/Levante.sln --verify-no-changes` |
| Rodar API | `dotnet run --project src/api/host/Levante.Api` (config `Mongo:ConnectionString` via user-secrets/env) |
| Emitir contrato OpenAPI | `dotnet run --project src/api/host/Levante.Api -- --emit-openapi "$PWD/src/web/openapi/levante.json"` (caminho absoluto: o `dotnet run` usa o dir do projeto como CWD) |
| Front dev | `npm run dev` (em `src/web`) |
| Front build | `npm run build` (em `src/web`) |
| Front lint | `npm run lint` (em `src/web`) |
| Gerar tipos do OpenAPI | `npm run gen:api` (em `src/web`, a partir de `openapi/levante.json`) |

Versões de pacote NuGet são centralizadas em `Directory.Packages.props` (Central Package Management). Não declare versão em `.csproj` nem invente versão fora do arquivo central.

## Esteira (gates de CI)

Sequência fixa. Nada vira `raised` (produção) sem passar todos.

| Estágio | Gate |
|---------|------|
| rough-cut | compila · warnings as errors · CPM consistente |
| dress | `dotnet format` limpo · analyzers + arch tests verdes · gitleaks limpo |
| polish | testes verdes (inclui isolamento e o teste de privilégio mínimo do Mongo) · cobertura mínima no domínio |
| raise | SAST sem alerta crítico · NuGet audit limpo · SBOM gerado · publish/containeriza |

Guardrails que travam o build: Central Package Management, `Directory.Build.props` com Nullable + TreatWarningsAsErrors + analyzers (SonarAnalyzer, Meziantou, BannedApiAnalyzers), `.editorconfig`, projeto de arch tests (NetArchTest) garantindo `Domain` sem dependência de `Infrastructure`, e pre-commit (Husky.NET) rodando format + gitleaks.

## Fluxo de trabalho com o agente

1. Em tarefa não trivial, entregue um PLANO primeiro (estrutura, pacotes com licença de cada um, desenho do CI, lista de arquivos). Aguarde aprovação antes de implementar.
2. Commits pequenos e atômicos. Conventional Commits em PT (ex.: `feat(conteudo): adiciona endpoint de listagem de artigos`).
3. Garanta o CI verde ao final de cada tarefa.
4. Não edite arquivos sensíveis (segredos, IaC de produção, migrations já aplicadas) sem confirmação explícita.
5. Se algo exigir uma decisão ainda aberta (ver GAPs), não assuma: deixe a opção mais simples, marque com `TODO` e sinalize.

## Pacotes a evitar (licença)

| Evitar | Motivo | Usar no lugar |
|--------|--------|---------------|
| MediatR | comercial em versões novas | handler direto por ora; avaliar Wolverine |
| AutoMapper | comercial | Mapster ou mapeamento manual |
| FluentAssertions v8 | passou a ser paga | Shouldly (ou travar v7) |
| QuestPDF | Community gratuita só abaixo do limite de receita | manter sob o limite; reavaliar se crescer |

Stack de teste: xUnit + Testcontainers (Mongo real, não mock) + Shouldly + Bogus + WireMock.NET + Verify.

## Fora de escopo / GAPs (não assumir decisão)

Itens em aberto. Mantenha o mais simples e marque com `TODO`:

- Mediator (GAP-F): sem lib por enquanto (handler chamado direto); decisão no spike que antecede a fatia do Outbox.
- Nível de assinatura de documentos (1 a 4): fora da Fatia 0.
- Profundidade do modelo científico (template vs DOI/ORCID): fora da Fatia 0.
- WhatsApp Cloud API: por ora só click-to-chat (`wa.me`).
- Contrato de eventos com o Hiram (GAP-I): o Felipe define no spike que antecede a fatia do Outbox, junto com GAP-F.
- Domínio do site (GAP-A): pendente; bloqueia a fatia de lançamento. Não assumir URL base definitiva (sempre via `SITE_URL`/env).

Decididos (não são mais GAPs): idioma PT-only (GAP-H) e hospedagem Azure Container Apps (GAP-J).

O roadmap de fatias vigente está em `docs/roadmap.md`.

Nota: alguns pontos da convenção (verbo de Command em PT, métodos de Repository em EN, contexto em PT) seguem o default acordado. Se o Felipe pedir para inverter algum, este arquivo é a fonte a atualizar.
