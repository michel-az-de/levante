"use client";

import { useState } from "react";
import { useIdioma } from "@/lib/i18n/IdiomaProvider";

export type Aba = { id: string; rotulo: string; comando: string };

/** Abas de instalacao (docker/source) com botao de copiar. Sem lib. */
export function CodeTabs({ abas }: { abas: readonly Aba[] }) {
  const { idioma } = useIdioma();
  const [ativa, setAtiva] = useState(abas[0]?.id ?? "");
  const [copiado, setCopiado] = useState(false);
  const abaAtual = abas.find((aba) => aba.id === ativa) ?? abas[0];

  async function copiar() {
    if (!abaAtual) {
      return;
    }
    try {
      await navigator.clipboard.writeText(abaAtual.comando);
      setCopiado(true);
      window.setTimeout(() => setCopiado(false), 1600);
    } catch {
      // Clipboard indisponivel (contexto nao-seguro): ignora sem quebrar a UI.
    }
  }

  return (
    <div className="mx-auto mt-9 max-w-[660px] overflow-hidden rounded-produto-lg border border-produto-line2 bg-produto-bg1 text-left">
      <div role="tablist" className="flex border-b border-produto-line bg-produto-bg2">
        {abas.map((aba) => (
          <button
            key={aba.id}
            type="button"
            role="tab"
            aria-selected={aba.id === ativa}
            onClick={() => setAtiva(aba.id)}
            className={`px-[18px] py-[11px] font-produto-mono text-[12.5px] transition-colors ${
              aba.id === ativa
                ? "text-produto-jade shadow-[inset_0_-2px_0_var(--color-produto-jade)]"
                : "text-produto-dim hover:text-produto-fg"
            }`}
          >
            {aba.rotulo}
          </button>
        ))}
      </div>
      <div className="flex items-start justify-between gap-3.5 p-[18px]">
        <pre className="overflow-x-auto font-produto-mono text-[13px] leading-[1.7] break-words whitespace-pre-wrap text-produto-fg">
          {abaAtual?.comando}
        </pre>
        <button
          type="button"
          onClick={() => void copiar()}
          className="flex-none rounded-md border border-produto-line2 bg-produto-bg3 px-2.5 py-1.5 font-produto-mono text-[11px] text-produto-dim transition-colors hover:border-produto-brass hover:text-produto-fg"
        >
          {copiado
            ? idioma === "en"
              ? "copied"
              : "copiado"
            : idioma === "en"
              ? "copy"
              : "copiar"}
        </button>
      </div>
    </div>
  );
}
