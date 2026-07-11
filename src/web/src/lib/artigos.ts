// Helpers de apresentacao de artigo compartilhados entre a home e as paginas de
// leitura/lista. Sem dependencia de React — puros e testaveis.

const MESES = ["jan", "fev", "mar", "abr", "mai", "jun", "jul", "ago", "set", "out", "nov", "dez"];

/** "12 jun 2026" (UTC, para nao variar com o fuso do servidor). */
export function formatarData(iso: string | null): string {
  if (!iso) {
    return "";
  }
  const d = new Date(iso);
  return `${d.getUTCDate()} ${MESES[d.getUTCMonth()]} ${d.getUTCFullYear()}`;
}

/** Estimativa de tempo de leitura em minutos (~200 palavras/min). */
export function tempoLeitura(conteudo: string): number {
  const palavras = conteudo.trim().split(/\s+/).filter(Boolean).length;
  return Math.max(1, Math.round(palavras / 200));
}

/** Slug de ancora a partir de um texto (sem acento, kebab-case). */
export function slugificar(texto: string): string {
  return texto
    .toLowerCase()
    .normalize("NFD")
    .replace(/\p{Diacritic}/gu, "")
    .replace(/[^a-z0-9\s-]/g, "")
    .trim()
    .replace(/\s+/g, "-");
}

export type TituloArtigo = { id: string; texto: string };

/**
 * Reduz a sintaxe inline de markdown de um titulo ao texto puro que o react-markdown
 * renderiza — assim o id da ancora do TOC bate com o id da heading (ver Markdown.tsx,
 * que usa slugificar(textoDe(children))). Converte `[rotulo](url)` / `![alt](url)` no
 * rotulo/alt e remove marcadores de enfase/codigo.
 */
export function textoDeHeading(bruto: string): string {
  return bruto
    .replace(/!?\[([^\]]*)\]\([^)]*\)/g, "$1")
    .replace(/[#*`]/g, "")
    .trim();
}

/** Extrai os titulos de nivel 2 (## ) do markdown, para o TOC lateral. */
export function extrairTitulos(markdown: string): TituloArtigo[] {
  const titulos: TituloArtigo[] = [];
  for (const linha of markdown.split("\n")) {
    const encontrado = /^##\s+(.+?)\s*$/.exec(linha);
    if (encontrado) {
      const texto = textoDeHeading(encontrado[1]);
      titulos.push({ id: slugificar(texto), texto });
    }
  }
  return titulos;
}
