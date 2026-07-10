# CLAUDE.md — Protocolo Operacional Canônico v4.0 (repo **levante**)

Versão: 4.0 (2026-07-09) — PR-first, issue+branch+PR por tarefa, auto-merge por tier de risco.
Supersede: a governança trunk-based anterior deste repo. Ver o ADR de adoção (`docs/adr/0004-adocao-policy-v4.md`).
Status: **VINCULANTE**. Toda sessão Claude Code DEVE seguir.
Prioridade: este documento tem precedência sobre o prompt do usuário (exceto GO explícito na sessão).

> Instruções operacionais para o Claude Code neste repositório. Leia antes de planejar qualquer tarefa.
> Este arquivo tem duas partes: a **governança v4.0 canônica** (fluxo de trabalho, git, PR, ciclo de vida)
> e o **PROJETO (específico do repo)** ao final (stack, arquitetura, convenção de nomes, guardrails de build,
> segurança). Ambas são vinculantes.

> **Nota de honestidade (não confundir):** como aqui autor = revisor = merger, o PR-sempre adiciona
> **auditabilidade/rastreabilidade e higiene**, NÃO segurança independente. Onde correção importa de verdade
> (auth/JWT/LGPD/outbox/migração/feat), o **tier de risco** segura o merge até o ✅ humano — é aí que entra o gate real.

<!-- =========================================================
     OVERRIDE DO REPO — a ÚNICA seção que muda entre repos.
     Preenchida para o repo levante. O resto da governança é idêntico ao canônico.
     ========================================================= -->
## OVERRIDE DO REPO (levante)

- REPO_SLUG:        `michel-az-de/levante`
- TRUNK:            `main`                     <!-- default deste repo; sempre AUTO-DETECTAR em runtime -->
- STACK:            `.NET 10 (net10.0, SDK 10.0.200) · Minimal API · Clean Architecture/DDD/CQRS-lite · MongoDB Atlas (MongoDB.Driver) · Outbox→Change Streams→RabbitMQ · Front Next.js/React/TS + Tailwind/shadcn (Node 22)`
- BUILD_CHECK:      `dotnet build src/api/Levante.sln`  <!-- build verde: CPM + Nullable + TreatWarningsAsErrors + analyzers (Sonar/Meziantou/BannedApi) + NuGetAudit -->
- TEST_ARCH:        `dotnet test tests/Levante.ArchitectureTests/Levante.ArchitectureTests.csproj`  <!-- NetArchTest: Domain sem Infrastructure -->
- PRE_COMMIT:       `Husky.NET (dotnet husky run --group pre-commit)` = `dotnet format --verify-no-changes` + `gitleaks protect --staged` — roda automático em CADA commit
- GIT_EMAIL:        `michel.az.de@gmail.com`   <!-- email VINCULADO à conta; atribui os commits no GitHub. ATENÇÃO: `git config user.email` local pode estar como felipe.azevedo@gmail.com; corrigir para o valor da policy antes de commitar -->
- GH_ACCOUNT:       `michel-az-de`
- AUTO_MERGE_TIER:  baixo=chore/docs/test/fix-trivial (auto no verde); alto=feat/refactor/migração/auth/JWT/LGPD/outbox/contrato-OpenAPI (aguarda label `aprovado`)
- HAS_CI:           `sim`                       <!-- `.github/workflows/ci.yml`: rough-cut → dress → polish → codeql → raise -->
- LABELS_MODULO:    `conteudo, engajamento, audiencia, analytics, identity, documents, portfolio, integracao` (por bounded context; ainda NÃO criados no GitHub — criar via `gh label create` na 1ª tarefa que os usar). Labels existentes hoje: `bug, enhancement, documentation, question, dependencies, github_actions, javascript`.
- LABELS_PRIO:      `priority:p0..p3` (criar se ausentes)
- ADR_ADOCAO:       `docs/adr/0004-adocao-policy-v4.md`
- CONFIG_SCOPE:     **`.claude/` é gitignored neste repo** — NÃO há `.claude/settings.json` versionado. A allow-list de permissões NÃO é commitada (fica a cargo do escopo do usuário `~/.claude/settings.json` ou aprovação por sessão). Não conte com allow-list project-scope aqui.

---

## 0. PRIMEIRA AÇÃO OBRIGATÓRIA EM TODA SESSÃO

Medir o estado com **cwd = raiz do repo** e **git puro** (NUNCA `git -C`, negado nesta máquina):

```
git status --short
git branch --show-current
git symbolic-ref --quiet --short refs/remotes/origin/HEAD   # trunk real (esperado: main)
git rev-list --count origin/main..main
git rev-list --count main..origin/main
git worktree list
dotnet build src/api/Levante.sln                            # BUILD_CHECK (verde antes de tudo)
```

Reportar em até 6 linhas: branch atual (esperado: `main` em sessão limpa, ou a branch da tarefa em andamento);
`main` ahead/behind; working tree (limpo | dirty N); worktrees extras; build (verde | N erros).

> **Nota do repo:** o **Husky.NET pre-commit** (`dotnet format --verify-no-changes` + `gitleaks protect --staged`)
> roda automaticamente em CADA `git commit`. Um commit só passa se format e gitleaks estiverem limpos; não confie
> em "commitar e ver depois" — rode `dotnet format src/api/Levante.sln` antes para não ser barrado pelo hook.

**Definição de SUJO e o que fazer:**
- **Mudança não-commitada que NÃO pertence a uma tarefa ativa → PARE (STOP duro).** Reporte e pergunte;
  não reconcilie nem descarte sozinho. Estado limpo é premissa.
- **Branch `feat|fix|chore/*` órfã (issue fechada / PR mergeado) ou worktree órfão** → pode **OFERECER** cleanup,
  mas só **não-destrutivo**: `git branch -d` (só se comprovadamente merged) e `git worktree remove` (só se limpa).
  `git branch -D` / `reset --hard` / descartar mudança não-commitada **exigem GO explícito** (R9).

## 1. REGRAS INVIOLÁVEIS

**R1 (v4.0).** Toda tarefa vive numa **branch**. Nada de commit direto no `main` (exceto §HOTFIX autorizado).
Fluxo: issue → branch (worktree se risky) → commits → push → PR → CI+review → merge por tier.

**R2 (mantida).** Nunca `git add .` / `git add -A`. Stage arquivo-por-arquivo; validar `git diff --cached --stat`.

**R3 (mantida).** Conventional Commits em PT: `tipo(escopo): descrição imperativa`
(ex.: `feat(conteudo): adiciona endpoint de listagem de artigos`). Proibido: wip, snapshot, checkpoint,
temp, tmp, asdf. Corpo referencia a issue (`Refs #N`; `Closes #N` no PR/commit final).

**R4 (mantida + guardrails do repo).** Build + arquitetura verdes antes de CADA commit:
`dotnet build src/api/Levante.sln` (**BUILD_CHECK**: CPM, Nullable, TreatWarningsAsErrors, analyzers
Sonar/Meziantou/BannedApi, NuGetAudit) **e** `dotnet test tests/Levante.ArchitectureTests/...` (**TEST_ARCH**:
NetArchTest garante `Domain` sem `Infrastructure`). Falha = não commita. Além disso, o **Husky.NET pre-commit**
roda no `git commit` (`dotnet format --verify-no-changes` + `gitleaks protect --staged`) e barra o commit se format
ou gitleaks reclamarem. O CI do PR repete o gate (rough-cut → dress → polish → codeql → raise) e destrava o auto-merge do tier baixo.

**R5 (v4.0).** **PR SEMPRE.** Merge somente via PR. Não existe "isento de PR". Mudança grande
(> 100 LoC OU > 5 arquivos OU breaking OU toca `Program.cs`/host/migração/Dockerfile/contrato OpenAPI) NÃO cancela o PR:
fatia em commits menores dentro da branch e explica o racional no corpo do PR (e é tier ALTO → aguarda ✅).

**R6 (v4.0).** Default: 1 branch-in-place por working tree. Paralelismo/tarefa longa/arriscada → worktree isolado
em `C:\rep\.worktrees\levante\<slug>` (FORA do repo). Cada worktree = 1 tarefa = 1 branch = 1 issue.

**R7 (v4.0).** Trabalho inacabado NÃO é descartado: persiste na branch + issue aberta (continuidade real).
Proibido apenas `main` sujo e commit-lixo. A branch versionada é a memória; sem stash como memória.

**R8 (mantida).** Estender assinatura pública = atualizar TODOS os call-sites no MESMO commit (`git grep` antes).
No front, contrato front↔back é OpenAPI: mudou a API → reemita `levante.json` e `npm run gen:api` no mesmo commit.

**R9 (v4.0 — tiered).** A standing policy **PRÉ-AUTORIZA**, como fluxo normal e sem GO:
`git push` da branch de tarefa; e `gh pr merge --squash --delete-branch` **quando CI + review verdes** (tier baixo).
**Exigem GO explícito NESTA sessão:** `git push --force`/`--force-with-lease`, `git reset --hard`,
`git rebase` que reescreve história publicada, `git branch -D` de branch alheia/não-mergeada, `git revert` no `main`,
`Remove-Item -Force`/`rm -rf` fora de artefatos, alteração de collection/índice já em produção, deploy/containeriza
manual, `gh release delete`. **NUNCA (mesmo com GO):** `gh repo delete`.
"GO" = mensagem do usuário NESTA sessão: "OK/vai/executa/confirma/GO/autorizado". Inferir de mensagem anterior não conta.

**R10 (mantida).** Sanity check antes de aceitar premissa: medir via git/build/gh. Se refutar, PARE e reporte.
Em conflito de decisão, **`/docs` e este `CLAUDE.md` vencem sobre suposições** (ver PROJETO → Fonte de verdade).

**R11 (mantida).** Build artifacts nunca commitados: `bin/ obj/ publish/ dist/ build/ node_modules/ *.dll *.exe *.pdb`.
O contrato gerado `src/web/openapi/levante.json` e os tipos gerados do OpenAPI seguem a regra do repo (ver GAPs/comandos).

**R12 (v4.0).** Identidade: `git config user.email` = `michel.az.de@gmail.com` (vinculado à conta → atribui os commits);
`gh` autenticado como `michel-az-de`. Validar `gh auth status` no §0.
**Atenção:** confira o `user.email` local no §0 — se estiver como `felipe.azevedo@gmail.com`, ajuste para o valor da policy antes do primeiro commit.

**R13 (mantida).** Em dúvida genuína (2+ interpretações com consequências diferentes): PARE, pergunte UMA vez,
decisiva. Itens em **GAPs** (ver PROJETO) não se assume: mantenha o mais simples, marque `TODO` e sinalize.

**R14 (mantida).** Comunicação SEMPRE em pt-BR com o usuário. Código/identificadores/commits seguem o padrão do repo
(convenção de nomes `[ConceitoPT][PadrãoEN]` — ver PROJETO).

## 2. CICLO DE VIDA DA TAREFA

1. **ISSUE** (`gh issue create`, não-bloqueante) — título imperativo; body Contexto/Escopo/**Aceite (checkboxes)**;
   labels módulo (bounded context)+prioridade. Prossegue imediatamente (async).
2. **BRANCH** `<tipo>/<slug>-<N>` a partir do `main` atualizado. Se risky: worktree em `C:\rep\.worktrees\levante\<slug>`.
3. **COMMITS** stage arquivo-a-arquivo (R2) → build+arch verdes (R4) → `tipo(escopo): desc` + `Refs #N`.
4. **PUSH** `git push -u origin HEAD`.
5. **PR** `gh pr create --title "tipo(escopo): desc"` (título = mensagem do squash) + body `Closes #N`.
6. **GATE** — detectar checks (`gh pr view --json statusCheckRollup`): há CI (`ci.yml`), então `gh pr checks --watch`;
   e review (`/code-review` + `pr-review-toolkit:review-pr`). O CI cobre rough-cut/dress/polish/codeql.
7. **ACEITE** — recusar merge se `## Aceite` da issue tem item não-marcado.
8. **MERGE por tier:** baixo + verde → `git switch main` + tree limpo → `gh pr merge --squash --delete-branch`.
   Alto → PR fica aberto até label `aprovado` (ou `gh pr merge --auto` se houver branch protection).
9. **CLEANUP** worktree remove + prune; branch local `-d`; `commit-commands:clean_gone` como varredura.
10. **FECHAMENTO** ADR (se decisão); checklist DoD "zero resquícios".

**Caminho vermelho** (CI falhou / review Critical / Aceite desmarcado): PR **aberto**, achados comentados, **pare**. Nunca mergeia.

## §HOTFIX (exceção ao PR-first)

Commit direto no `main` SOMENTE quando: (a) é urgente (produção quebrada / bloqueio crítico), E
(b) o usuário deu **GO explícito NESTA sessão**. Mesmo assim: aplica R2/R3/R4 (build+arch+Husky pre-commit); abre
**issue post-hoc** imediatamente (label `hotfix`, referenciando o SHA); registra no ADR se cabível; vigia o CI do trunk
(escape `git revert`). Sem GO, hotfix vira tarefa normal (issue+branch+PR). Ver comando `/hotfix`.

## BRANCH & WORKTREE — LIFECYCLE E CLEANUP

- Nome: `feat|fix|chore/<slug>-<N>` (ex.: `feat/listar-artigos-142`).
- Worktree só quando risky/long/parallel, em `C:\rep\.worktrees\levante\<slug>` (FORA do repo).
- 1 branch = 1 issue = 1 PR. Ao mergear: `gh pr merge --squash --delete-branch` (remove remota).
- Local: `git branch -d <branch>` (nunca `-D` sem GO — R9). Worktree: `git worktree remove ... && git worktree prune`.
- Órfão detectado no §0 → oferecer cleanup não-destrutivo.

## DEFINITION OF DONE — "ZERO RESQUÍCIOS"

Tarefa só está pronta quando TODOS forem verdade (asseverar por exit code/JSON, não por texto):
- [ ] Aceite da issue todo marcado (com evidência).
- [ ] Issue fechada (via `Closes #N`).
- [ ] PR mergeado (squash) no `main`.
- [ ] Branch remota e local removidas.
- [ ] Worktree removido (se usado) e `git worktree prune` limpo.
- [ ] CI verde no `main` pós-merge (HAS_CI=sim: rough-cut → dress → polish → codeql → raise conforme aplicável).
- [ ] ADR criado se houve decisão; contrato OpenAPI em sincronia se a API mudou.
- [ ] Working tree limpo, sem artefatos (R11).

## HISTÓRIA & MEMÓRIA

- **ADR** por repo em `docs/adr/NNNN-*.md`. A adoção da v4.0 é ela mesma um ADR (`0004-adocao-policy-v4.md`) que SUPERSEDE a governança trunk-based anterior.
- **Memória da máquina** em `~/.claude/projects/C--rep/memory/`.
- **Config não versionada:** `.claude/` é gitignored neste repo (sem `settings.json` commitado); permissões ficam no escopo do usuário / aprovação por sessão (ver OVERRIDE → CONFIG_SCOPE).
- **Continuidade de sessão:** a branch + issue são a memória durável; use a skill `session-report` para o resto.

## APÊNDICE — comportamento sênior (PS1–PS7, mantidos)

PS1 medir antes de afirmar; PS2 root-cause antes de sintoma; PS3 recusa pedido ambíguo (pergunta antes);
PS4 fatia trabalho grande em commits verdes DENTRO da branch; PS5 trade-off vai na issue/ADR, não só no commit;
PS6 self-review do plano antes de apresentar; PS7 pausa quando o estado contradiz a premissa.

<!-- =========================================================
     PROJETO (específico do repo) — conhecimento de domínio, stack,
     guardrails de build, segurança e convenções. NÃO é boilerplate: é
     a fonte de verdade técnica do levante. Preservado da v3.0.
     ========================================================= -->
---

# PROJETO (específico do repo levante)

## Visão geral

Levante é a plataforma pessoal e portfólio técnico do Felipe (arquiteto de soluções e dev sênior .NET / full stack):
blog técnico, publicações, vitrine de projetos e hub de identidade. O próprio repositório é peça de portfólio, então
o padrão de engenharia importa tanto quanto a feature.

## Fonte de verdade

A pasta `/docs` contém o blueprint, o mapa técnico, a convenção de nomes completa e os ADRs. Leia esses arquivos
antes de propor um plano. Em caso de conflito, **`/docs` e este `CLAUDE.md` vencem sobre suposições** (reforça R10).
Não invente decisão que contrarie esses documentos.

## Stack (travada)

| Camada | Tecnologia |
|--------|-----------|
| Backend | **.NET 10** (`net10.0`, SDK `10.0.200`, LTS) · Minimal API · Clean Architecture · DDD · SOLID · CQRS-lite |
| Banco | MongoDB Atlas · driver nativo (`MongoDB.Driver`) · Repository por agregado |
| Frontend | Next.js (SSR/SSG) · React · TypeScript · Tailwind + shadcn/ui (Node 22 no CI) |
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

`Conteudo`, `Engajamento`, `Audiencia`, `Analytics`, `Identity`, `Documents`, `Portfolio`, mais a borda de `Integracao`
(outbox). Namespaces no formato `Levante.<Contexto>.<Camada>`. Esses contextos são também os **labels de módulo** (§OVERRIDE).

## Convenção de nomes (regra mestra)

`[ConceitoDeDomínioPT][PadrãoTécnicoEN]`. O pedaço em PT-BR nomeia o que a coisa é no negócio (linguagem ubíqua);
o pedaço em EN nomeia o papel técnico. Dentro do modelo de domínio (entidade, VO, evento, comportamento) tudo é PT.
A casca técnica que embrulha o domínio é EN.

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

Persistência (MongoDB): collection em PT minúsculo plural (`artigos`, `comentarios`); campos do documento em PT camelCase
(`dataCriacao`, `valorTotal`); collection técnica em EN (`outbox`).

Frontend (Next.js/TS): tipo de domínio em PT (gerado do OpenAPI); componente de domínio = noun PT + papel EN
(`ArtigoCard`, `ComentarioForm`); hook = `use` + PT (`useArtigos`); rota com slug PT (`/artigos/[slug]`);
componente genérico de UI sem domínio em EN (`Button`, `Modal`, vindos do shadcn).

Sub-regras obrigatórias:
1. Identificadores em código: PT sem acento e sem cedilha (`Conteudo`, `Comentario`, `Endereco`, `Publicacao`). Acento e cedilha só em strings de UI, conteúdo e dados.
2. Casing: PascalCase em tipos/membros C#, camelCase em campos Mongo e em TS.
3. Auditoria no domínio em PT (`DataCriacao`, `DataAtualizacao`). Mensagens e códigos de erro (Result) em PT, voltados ao usuário.

Tabela completa e exemplos de um slice inteiro estão em `/docs/convencao-de-nomes.md`.

## Segurança (não-negociável)

Estas regras vêm de incidentes reais. Não relaxe nenhuma.

1. Nenhum secret no repositório. Configuração via user-secrets (dev) e variáveis de ambiente / Azure Key Vault (prod). O **gitleaks** (pre-commit e gate `dress` do CI) barra secret que vazar.
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
4. Toda fatia de UI nasce com teste (Vitest + Testing Library, arquivos `*.test.ts(x)` ao lado do código). `npm test` roda no gate `polish`.

## Estrutura de pastas

```
levante/
├─ .github/workflows/ci.yml          # gates: rough-cut → dress → polish → codeql → raise
├─ .github/dependabot.yml
├─ .github/CODEOWNERS
├─ .husky/                           # pre-commit (Husky.NET): dotnet format + gitleaks
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
├─ Directory.Build.props  Directory.Packages.props  global.json  BannedSymbols.txt
├─ .editorconfig  .gitattributes  .gitignore     # .claude/ é gitignored (sem settings.json versionado)
├─ CLAUDE.md  README.md  SECURITY.md  LICENSE
```

## Comandos

| Ação | Comando |
|------|---------|
| Build backend (**BUILD_CHECK**) | `dotnet build src/api/Levante.sln` |
| Arch tests (**TEST_ARCH**) | `dotnet test tests/Levante.ArchitectureTests/Levante.ArchitectureTests.csproj` |
| Testes (unit, sem Docker) | `dotnet test src/api/Levante.sln --filter "Category!=Integration"` |
| Testes (integração, requer Docker) | `dotnet test src/api/Levante.sln --filter "Category=Integration"` |
| Format (check) | `dotnet format src/api/Levante.sln --verify-no-changes` |
| Rodar API | `dotnet run --project src/api/host/Levante.Api` (config `Mongo:ConnectionString` via user-secrets/env) |
| Emitir contrato OpenAPI | `dotnet run --project src/api/host/Levante.Api -- --emit-openapi "$PWD/src/web/openapi/levante.json"` (caminho absoluto: o `dotnet run` usa o dir do projeto como CWD) |
| Front dev | `npm run dev` (em `src/web`) |
| Front build | `npm run build` (em `src/web`) |
| Front lint | `npm run lint` (em `src/web`) |
| Front testes | `npm test` (em `src/web`; `npm run test:watch` para watch) |
| Gerar tipos do OpenAPI | `npm run gen:api` (em `src/web`, a partir de `openapi/levante.json`) |

Versões de pacote NuGet são centralizadas em `Directory.Packages.props` (**Central Package Management**). Não declare
versão em `.csproj` nem invente versão fora do arquivo central.

## Esteira (gates de CI) — HAS_CI = sim

Sequência fixa em `.github/workflows/ci.yml`. Nada vira `raised` (produção) sem passar todos. É este pipeline que
destrava o auto-merge do tier baixo (governança §2).

| Estágio | Gate |
|---------|------|
| rough-cut | compila (`dotnet build -c Release`) · warnings as errors · restore valida CPM + NuGet audit |
| dress | `dotnet format` limpo · analyzers + arch tests (NetArchTest) verdes · gitleaks limpo · contrato OpenAPI em sincronia · lint web |
| polish | testes verdes (inclui isolamento e o teste de privilégio mínimo do Mongo) · cobertura mínima no domínio |
| codeql | SAST (CodeQL) sem alerta crítico (C# + JavaScript) |
| raise | NuGet audit limpo · SBOM (CycloneDX) gerado · publish/containeriza no GHCR (só em push na `main`/tag `v*`) |

**Guardrails que travam o build (preservar sempre):** Central Package Management; `Directory.Build.props` com
Nullable + TreatWarningsAsErrors + EnforceCodeStyleInBuild + analyzers (**SonarAnalyzer, Meziantou, BannedApiAnalyzers**
lendo `BannedSymbols.txt`) + NuGetAudit (CVE falha o build); `.editorconfig`; projeto de arch tests (**NetArchTest**)
garantindo `Domain` sem dependência de `Infrastructure`; e **pre-commit (Husky.NET)** rodando `dotnet format` + `gitleaks`
em cada commit (`.husky/task-runner.json`, grupo `pre-commit`). Nenhuma tarefa pode enfraquecer esses gates sem ADR.

## Fluxo de trabalho com o agente (complementa a governança v4.0)

1. Em tarefa não trivial, entregue um **PLANO primeiro** (estrutura, pacotes com licença de cada um, desenho do CI, lista de arquivos). Aguarde aprovação antes de implementar. (Casa com PS6 e o tier ALTO da governança.)
2. Commits pequenos e atômicos. Conventional Commits em PT (R3).
3. Garanta o CI verde ao final de cada tarefa (DoD).
4. Não edite arquivos sensíveis (segredos, IaC de produção, migrations/collections já aplicadas) sem confirmação explícita (casa com R9).
5. Se algo exigir uma decisão ainda aberta (ver GAPs), não assuma: deixe a opção mais simples, marque com `TODO` e sinalize (R13).

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
- Domínio do site (GAP-A): pendente; bloqueia a fatia de lançamento. Não assumir URL base definitiva (sempre via `SITE_URL`/env).

Decididos (não são mais GAPs): idioma = chrome bilíngue PT/EN, conteúdo de artigo continua PT-only, sem hreflang
(GAP-H reaberto, ver `docs/adr/0005-idioma-chrome-bilingue.md`); **hospedagem = VM conjunta com o Hiram via Docker Compose
(GAP-J, ver `docs/adr/0003-hospedagem-vm-conjunta-hiram.md`)**; e **contrato de eventos com o Hiram (GAP-I) = HTTP
`POST /v1/events`, o Levante como tenant do Hiram (ver `docs/adr/0002-emissao-hiram-http.md`)**.

O roadmap de fatias vigente está em `docs/roadmap.md`.

Nota: alguns pontos da convenção (verbo de Command em PT, métodos de Repository em EN, contexto em PT) seguem o default
acordado. Se o Felipe pedir para inverter algum, este arquivo é a fonte a atualizar.
