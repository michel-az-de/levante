import type { ReactNode } from "react";

/**
 * Renderiza os dois idiomas no HTML; o atributo data-idioma no <html> decide
 * via CSS (globals.css) qual aparece. Sem JS e sem mismatch de hidratacao —
 * funciona ate com JavaScript desligado. So o chrome/UI usa isto; o conteudo
 * de artigo continua so em PT (ADR 0005).
 */
export function Idioma({ pt, en }: { pt: ReactNode; en: ReactNode }) {
  return (
    <>
      <span data-idioma-pt="" lang="pt-BR">
        {pt}
      </span>
      <span data-idioma-en="" lang="en">
        {en}
      </span>
    </>
  );
}
