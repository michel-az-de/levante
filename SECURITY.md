# Politica de Seguranca

A seguranca do Levante e levada a serio: parte das regras nasceu de incidentes
reais (ver `CLAUDE.md`, secao Seguranca). Agradecemos relatos responsaveis.

## Versoes suportadas

O projeto esta em construcao (Fatia 0 / walking skeleton). Apenas o branch
`main` recebe correcoes de seguranca por enquanto.

## Como reportar uma vulnerabilidade

**Nao** abra issue publica para falhas de seguranca. Use o canal privado do
GitHub:

1. Acesse a aba **Security** do repositorio.
2. Clique em **Report a vulnerability** (Private Vulnerability Reporting).
3. Descreva o impacto, os passos de reproducao e, se possivel, uma prova de
   conceito.

Resposta inicial em ate 5 dias uteis. Pedimos um prazo razoavel para correcao
antes de divulgacao publica (coordinated disclosure).

## Boas praticas adotadas

- Nenhum secret no repositorio (user-secrets em dev; variaveis de ambiente /
  Key Vault em producao).
- Conta de runtime do MongoDB com privilegio minimo, validada por teste e por
  self-check de boot que aborta o processo em producao se houver privilegio
  administrativo.
- Todo endpoint nasce com autorizacao explicita.
- Gates de CI: analyzers, arch tests, gitleaks, CodeQL (SAST) e NuGet audit.
- Dependabot para atualizacoes de dependencias (.NET, npm e GitHub Actions).
