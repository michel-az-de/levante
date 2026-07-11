import type { Bilingue } from "@/lib/i18n/textos";
import { Idioma } from "@/components/Idioma";
import { CabecalhoProduto, SecaoProduto } from "./SecaoProduto";

type Recurso = { icone: string; titulo: Bilingue; descricao: Bilingue };

const recursos: readonly Recurso[] = [
  {
    icone: "✎",
    titulo: { pt: "Editor calmo", en: "Calm editor" },
    descricao: {
      pt: "WYSIWYG que some quando você escreve e aparece quando precisa. Sem barra de ferramentas no caminho.",
      en: "WYSIWYG that disappears while you write and shows up when you need it. No toolbar in the way.",
    },
  },
  {
    icone: "⟐",
    titulo: { pt: "API-first", en: "API-first" },
    descricao: {
      pt: "Content API documentada em OpenAPI. Gere tipos, consuma de qualquer front-end. Headless de verdade.",
      en: "Content API documented in OpenAPI. Generate types, consume from any front-end. Truly headless.",
    },
  },
  {
    icone: "⇄",
    titulo: { pt: "Núcleo de eventos", en: "Event core" },
    descricao: {
      pt: "Outbox transacional. Publicou um artigo, um evento sai pela fila e alimenta sua notificação. Nada se perde.",
      en: "Transactional outbox. Publish an article, an event leaves through the queue and feeds your notifications. Nothing is lost.",
    },
  },
  {
    icone: "◫",
    titulo: { pt: "Clean Architecture", en: "Clean Architecture" },
    descricao: {
      pt: "DDD, CQRS, camadas honestas. Código que você consegue ler, testar e estender sem medo.",
      en: "DDD, CQRS, honest layers. Code you can read, test and extend without fear.",
    },
  },
  {
    icone: "⌖",
    titulo: { pt: "SEO e i18n", en: "SEO and i18n" },
    descricao: {
      pt: "Pronto pra SSG/ISR: sitemap, JSON-LD, RSS e múltiplos idiomas na origem do conteúdo.",
      en: "Ready for SSG/ISR: sitemap, JSON-LD, RSS and multiple languages at the content source.",
    },
  },
  {
    icone: "⛉",
    titulo: { pt: "Privacidade por padrão", en: "Privacy by default" },
    descricao: {
      pt: "Consentimento, anonimização de IP e retenção no núcleo. LGPD não é plugin, é fundação.",
      en: "Consent, IP anonymization and retention in the core. Privacy is not a plugin, it's foundation.",
    },
  },
];

/** Secao Recursos: grade de 6 cards. */
export function Recursos() {
  return (
    <SecaoProduto id="recursos">
      <CabecalhoProduto
        kicker={{ pt: "Recursos", en: "Features" }}
        titulo={{
          pt: "Tudo que uma publicação séria precisa, nada que atrapalhe.",
          en: "Everything a serious publication needs, nothing in the way.",
        }}
        subtitulo={{
          pt: "Construído como produto, não como template. Cada peça é desenhada pra durar e pra ser sua.",
          en: "Built as a product, not a template. Every piece designed to last and to be yours.",
        }}
      />
      <div className="grid grid-cols-1 gap-3.5 sm:grid-cols-2 lg:grid-cols-3">
        {recursos.map((recurso) => (
          <div
            key={recurso.titulo.pt}
            className="rounded-produto-lg border border-produto-line bg-produto-bg1 p-[22px]"
          >
            <div
              aria-hidden="true"
              className="mb-[15px] flex h-[38px] w-[38px] items-center justify-center rounded-produto-md bg-produto-jadeb text-[18px] text-produto-jade"
            >
              {recurso.icone}
            </div>
            <h3 className="mb-[7px] text-[17px] font-semibold text-produto-fg">
              <Idioma pt={recurso.titulo.pt} en={recurso.titulo.en} />
            </h3>
            <p className="text-[13.5px] leading-[1.55] text-produto-dim">
              <Idioma pt={recurso.descricao.pt} en={recurso.descricao.en} />
            </p>
          </div>
        ))}
      </div>
    </SecaoProduto>
  );
}
