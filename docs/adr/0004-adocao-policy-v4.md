# ADR-0004 — Adocao da Policy v4.0 (PR-first, issue-driven) no levante

- Status: Aceito
- Data: 2026-07-09
- Supersede: a governanca trunk-based anterior do repo levante (commit direto na `main`, sem PR)

## Contexto

Os repos em `C:\rep` sao desenvolvidos por Claude Code. A politica operacional anterior do levante era
trunk-based (commit direto no trunk `main`, sem PR, sem branches de tarefa). A direcao passou a exigir:
**PR sempre** (exceto hotfix urgente autorizado), fluxo alinhado ao GitHub (issue -> branch -> PR -> close),
limpeza de branches/worktrees ao terminar, e historico/memoria para continuidade entre sessoes.

O levante ja tinha **guardrails de build fortes** que precisam ser preservados integralmente sob a nova governanca:
Central Package Management, `Directory.Build.props` com Nullable + TreatWarningsAsErrors + analyzers (SonarAnalyzer,
Meziantou, BannedApiAnalyzers lendo `BannedSymbols.txt`) + NuGetAudit, arch tests (NetArchTest: `Domain` sem
`Infrastructure`), pre-commit Husky.NET (`dotnet format` + `gitleaks`) e o pipeline de CI
`rough-cut -> dress -> polish -> codeql -> raise`.

## Decisao

Adotar o **Protocolo Operacional Canonico v4.0** (ver `CLAUDE.md`): toda tarefa = issue + branch + PR,
com **auto-merge por tier de risco** (baixo = auto no verde; alto = feat/refactor/migracao/auth/JWT/LGPD/outbox/
contrato-OpenAPI aguarda label `aprovado`), worktrees fora do repo (`C:\rep\.worktrees\levante`), e Definition of Done
com criterio de Aceite verificavel.

Os guardrails de build do repo permanecem VINCULANTES e sao incorporados a regra R4 da governanca:
`dotnet build src/api/Levante.sln` (BUILD_CHECK) + `dotnet test tests/Levante.ArchitectureTests/...` (TEST_ARCH)
verdes antes de cada commit, com o Husky.NET pre-commit (format + gitleaks) rodando no `git commit`. O CI (HAS_CI=sim)
repete o gate e destrava o auto-merge do tier baixo. A fonte de verdade `/docs` + `CLAUDE.md` continua vencendo sobre suposicoes.

## Consequencias

- A `main` deixa de receber commit direto (salvo hotfix autorizado com issue post-hoc).
- Ganha-se auditabilidade (revert granular, trilha issue/PR), NAO seguranca independente (autor=revisor=merger);
  o gate real de correcao e o tier alto + aprovacao humana.
- Identidade de commit passa a `michel.az.de@gmail.com` (vinculado a conta -> atribui no GitHub). Se o `user.email`
  local estiver como `felipe.azevedo@gmail.com`, deve ser ajustado antes do primeiro commit sob a v4.0.
- Guardrails de build preservados 100%: CPM, Nullable/TreatWarningsAsErrors, analyzers (Sonar/Meziantou/BannedApi),
  NetArchTest, Husky.NET pre-commit (format + gitleaks) e a esteira de CI de 5 estagios.
- `.claude/` e gitignored neste repo: NAO ha `.claude/settings.json` versionado. A allow-list de permissoes nao e
  commitada (fica a cargo do escopo do usuario `~/.claude/settings.json` ou aprovacao por sessao).
- Labels de modulo por bounded context (`conteudo, engajamento, audiencia, analytics, identity, documents, portfolio,
  integracao`) devem ser criados no GitHub quando a primeira tarefa de cada contexto surgir.
- Automacao via `/tarefa-inicio`, `/tarefa-fim`, `/hotfix` + hooks SessionStart/Stop.
- O Dev Janitor pula repos fora do trunk (guard) e nao toca `C:\rep\.worktrees`.

## Alternativas consideradas

- Manter trunk-based anterior: rejeitada (nao atende PR-sempre nem a gestao por issue pedida).
- CLAUDE.md global unico: rejeitada (preferencia por politica por-repo com override, preservando o conhecimento
  especifico do levante: stack, convencao de nomes, guardrails).
- Auto-merge total sem tier: rejeitada (daria falsa sensacao de gate; alto risco sem aprovacao humana).
