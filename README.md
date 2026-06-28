# Levante

Plataforma pessoal e portfólio técnico de Felipe Michel de Azevedo, arquiteto de soluções e desenvolvedor sênior .NET / full stack. Blog técnico, publicações, vitrine de projetos e hub de identidade.

> Da pedra bruta à pedra polida.

## Stack

| Camada | Tecnologia |
|--------|-----------|
| Backend | .NET (LTS) · Minimal API · Clean Architecture · DDD · SOLID · CQRS-lite |
| Banco | MongoDB Atlas |
| Frontend | Next.js (SSR/SSG) · React · TypeScript · Tailwind + shadcn/ui |
| Mensageria | Outbox → RabbitMQ (entrega via Hiram) |
| Infra | Azure · GitHub Actions |

## Arquitetura

Monólito modular. Cada bounded context (`Conteudo`, `Engajamento`, `Audiencia`, `Analytics`, `Identity`, `Documents`, `Portfolio`) é um módulo com Clean Architecture própria. Notificações são responsabilidade de outro projeto (Hiram); aqui os eventos são publicados via Outbox.

```
Navegador → Next.js (SSR) → [OpenAPI] → .NET Minimal API → Domain → MongoDB
                                              └→ Outbox → RabbitMQ → Hiram → email/push/WhatsApp
```

## Documentação

| Arquivo | Conteúdo |
|---------|----------|
| `docs/blueprint.md` | Visão e arquitetura E2E |
| `docs/mapa-tecnico.md` | Todas as decisões técnicas com status |
| `docs/mapa-tecnico.html` | Versão visual do mapa (abrir no navegador) |
| `docs/convencao-de-nomes.md` | Convenção de nomes + slice de referência |
| `CLAUDE.md` | Guia operacional para desenvolvimento assistido |

## Qualidade e segurança

Esteira com gates em sequência: `rough-cut → dress → polish → raise`. Arch tests, analyzers, CodeQL, gitleaks, NuGet audit e testes de isolamento multi-tenant. Nada vai a produção sem passar todos os gates.

## Status

Em construção. Fatia 0 (walking skeleton) em andamento.

## Licença

O código-fonte é licenciado sob Apache-2.0 (ver `LICENSE`).

O nome "Levante", a identidade visual, o design system e todo o conteúdo editorial (artigos, publicações, textos) não estão cobertos pela licença e são de uso reservado. Reutilize os padrões de arquitetura à vontade; a marca e o conteúdo, não.
