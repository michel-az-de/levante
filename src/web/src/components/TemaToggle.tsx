"use client";

import { useCallback } from "react";
import { useIdioma } from "@/lib/i18n/IdiomaProvider";

const ATRIBUTO = "data-theme";
const CHAVE_STORAGE = "levante:tema";
type Tema = "dark" | "light";

/** Botao que alterna o tema claro/escuro do site (persistido em localStorage). */
export function TemaToggle({ className }: { className?: string }) {
  const { t } = useIdioma();

  const alternar = useCallback(() => {
    const atual: Tema =
      document.documentElement.getAttribute(ATRIBUTO) === "light" ? "light" : "dark";
    const novo: Tema = atual === "light" ? "dark" : "light";
    document.documentElement.setAttribute(ATRIBUTO, novo);
    try {
      localStorage.setItem(CHAVE_STORAGE, novo);
    } catch {
      // localStorage indisponivel (modo privado): a escolha vale so nesta sessao.
    }
  }, []);

  return (
    <button type="button" onClick={alternar} aria-label={t("alternarTema")} className={className}>
      ◐
    </button>
  );
}
