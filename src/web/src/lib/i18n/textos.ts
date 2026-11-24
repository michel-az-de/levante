// Dicionario de chrome bilingue (ADR 0005). So a casca/UI passa por aqui;
// o conteudo de artigo continua so em PT. A maior parte do texto estatico usa
// o componente <Idioma pt en /> inline no JSX; este mapa e para os poucos casos
// em que um Client Component precisa da string bruta (aria-label, cmd-k, title).
// Expandido pelas fatias de UI seguintes.

export type Idioma = "pt" | "en";
export type Bilingue = { pt: string; en: string };

export const textos = {
  mudarIdioma: { pt: "Mudar para inglês", en: "Switch to Portuguese" },
  alternarTema: { pt: "Alternar tema claro/escuro", en: "Toggle light/dark theme" },
  menuAbrir: { pt: "Abrir menu de navegação", en: "Open navigation menu" },
  menuFechar: { pt: "Fechar menu de navegação", en: "Close navigation menu" },
} as const satisfies Record<string, Bilingue>;

export type ChaveTexto = keyof typeof textos;
