import type { components } from "@/types/api";

/**
 * Tipo de dominio em PT, gerado do OpenAPI (nunca escrito a mao).
 * Ver docs/convencao-de-nomes.md (Frontend).
 */
export type Artigo = components["schemas"]["ArtigoResponse"];

export type Categoria = components["schemas"]["CategoriaResponse"];

export type Comentario = components["schemas"]["ComentarioResponse"];
