"use client";

import { useIdioma } from "@/lib/i18n/IdiomaProvider";
import { alternarTema } from "@/lib/tema";

/** Botao que alterna o tema claro/escuro do site (persistido em localStorage). */
export function TemaToggle({ className }: { className?: string }) {
  const { t } = useIdioma();
  return (
    <button
      type="button"
      onClick={alternarTema}
      aria-label={t("alternarTema")}
      className={className}
    >
      ◐
    </button>
  );
}
