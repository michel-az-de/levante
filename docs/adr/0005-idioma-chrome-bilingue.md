# ADR 0005 — Reabertura do GAP-H: chrome bilíngue PT/EN (conteúdo continua PT-only)

Status: **Aceito** · jul/2026 · Reabre o GAP-H registrado como "Resolvido → PT-only" em `docs/mapa-tecnico.md` e `docs/roadmap.md`

## Contexto

O GAP-H (idiomas) estava fechado como PT-only: sem toggle de idioma, sem rotas por locale, sem `hreflang` (`docs/mapa-tecnico.md`, `docs/blueprint.md` §15, `docs/roadmap.md`, `docs/plano-mvp-producao.md`). A decisão fazia sentido para o MVP original: traduzir o **corpo** dos artigos técnicos dobra o esforço editorial sem benefício claro para um blog PT-BR de nicho.

Ao planejar o redesign do site público (site pessoal do Felipe + landing do produto Levante), o Felipe decidiu reabrir a parte de apresentação dessa decisão: quer um toggle PT/EN funcional na casca da UI (nav, labels, textos estáticos de seção e — na landing do produto, cujo público inclui desenvolvedores avaliando adoção — também a copy de marketing). Reabrir uma decisão registrada como fechada exige ADR (`CLAUDE.md` R10 e DoD), daí este documento.

## Decisão

1. **Bilíngue é chrome/copy, não conteúdo.** O corpo de artigo (campo `Conteudo`, markdown, vindo da Content API) continua **só em português**, sem tradução, sem campo novo no agregado `Artigo`. O que passa a ser bilíngue é a interface ao redor: nav, labels, seções estáticas (Consultoria, Capacidades, Experiência, Contato no site pessoal; toda a copy da landing `/levante`).
2. **Sem rotas por locale, sem `hreflang`.** Não existe `/en/...`; a alternância é 100% no mesmo URL. Isso preserva a parte da decisão original que continua correta: não há conteúdo equivalente em outra URL, então `hreflang` não se aplica.
3. **`<html lang="pt-BR">` continua fixo** no documento inteiro — cada trecho traduzido leva seu próprio `lang` (`<span lang="en">`), preservando WCAG 3.1.2 (Language of Parts) sem precisar de rota nem negociação de idioma no servidor.
4. **Renderização dual no servidor, troca por CSS — não por JS pós-hidratação.** Os dois idiomas são emitidos no HTML por um Server Component (`<Idioma pt="" en="" />`); um atributo `data-idioma` no `<html>` (setado por um script inline anti-FOUC, mesmo mecanismo do toggle de tema) decide via CSS qual metade aparece (`[data-idioma="pt"] [data-idioma-en]{display:none}` e vice-versa). Zero mismatch de hidratação, funciona sem JS, sem lib de i18n (`next-intl` etc.) — o escopo é só chrome (~150-250 strings), sem rotas por locale nem plural/ICU que justifiquem uma lib.
5. **Persistência só em `localStorage`**, sem cookie: a escolha de idioma nunca é lida no servidor (o idioma inicial renderizado é sempre `"pt"`), então um cookie não traria nenhuma personalização de SSR — só complexidade de consentimento desnecessária.

## Consequências

- `docs/mapa-tecnico.md`, `docs/blueprint.md`, `docs/roadmap.md`, `docs/plano-mvp-producao.md`, `README.md` e `CLAUDE.md` deixam de descrever GAP-H como "PT-only" sem qualificação — passam a registrar "chrome bilíngue PT/EN, conteúdo PT-only, sem hreflang".
- Nenhum campo novo no agregado `Artigo` nem na Content API — o contrato OpenAPI (`levante.json`) não muda.
- `sitemap.ts`, RSS, JSON-LD e `robots.txt` não mudam: continuam refletindo só a versão PT (não há segunda URL/versão de conteúdo a indexar).
- Se no futuro alguém quiser traduzir o **corpo** de artigos de verdade (não só chrome), isso é um GAP novo, não coberto por esta ADR — exigiria campo(s) localizados no domínio, rotas por locale e `hreflang` de verdade.

## Alternativas consideradas

- Manter PT-only (não reabrir): rejeitada — o Felipe quer o toggle funcionando de verdade nos dois novos designs (site pessoal e landing do produto).
- i18n completo com lib (`next-intl`) + rotas `/en`: rejeitada por ora — desproporcional para ~150-250 strings de chrome sem conteúdo traduzido; reavaliar se o corpo de artigo também precisar ser traduzido um dia.
- Trocar o texto via JS após a hidratação (o padrão do mockup de referência usado como inspiração visual): rejeitada — causa flash de conteúdo errado (FOUC) e não funciona sem JS; a renderização dual + CSS resolve os dois problemas sem custo de hidratação.
