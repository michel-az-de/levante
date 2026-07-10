/** Filtro fuzzy simples da paleta de comandos (substring em pt ou en). Puro. */
export function filtrarComandos<T extends { pt: string; en: string }>(
  itens: readonly T[],
  busca: string,
): T[] {
  const q = busca.trim().toLowerCase();
  if (!q) {
    return [...itens];
  }
  return itens.filter(
    (item) => item.pt.toLowerCase().includes(q) || item.en.toLowerCase().includes(q),
  );
}
