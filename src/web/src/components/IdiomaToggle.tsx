"use client";

import { useIdioma } from "@/lib/i18n/IdiomaProvider";

/** Botao PT/EN que alterna o idioma de chrome (ADR 0005). */
export function IdiomaToggle({ className }: { className?: string }) {
  const { alternar, t } = useIdioma();
  return (
    <button type="button" onClick={alternar} aria-label={t("mudarIdioma")} className={className}>
      PT/EN
    </button>
  );
}
