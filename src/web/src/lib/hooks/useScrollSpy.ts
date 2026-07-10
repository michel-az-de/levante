"use client";

import { useEffect, useState } from "react";

/**
 * Funcao pura: dada a posicao (top, relativa ao viewport) de cada secao e um
 * limite, devolve a ultima secao cujo topo ja passou do limite. Extraida para
 * teste — o hook so faz a leitura do DOM e o wiring de scroll.
 */
export function escolherSecaoAtiva(
  secoes: readonly { id: string; top: number }[],
  limite: number,
): string | null {
  let atual: string | null = null;
  for (const secao of secoes) {
    if (secao.top <= limite) {
      atual = secao.id;
    }
  }
  return atual;
}

/** Marca a secao ativa conforme o scroll, para destacar o item do nav. */
export function useScrollSpy(ids: readonly string[]): string | null {
  const [ativa, setAtiva] = useState<string | null>(null);

  useEffect(() => {
    function medir() {
      const limite = window.innerHeight * 0.38;
      const secoes = ids.flatMap((id) => {
        const el = document.getElementById(id);
        return el ? [{ id, top: el.getBoundingClientRect().top }] : [];
      });
      setAtiva(escolherSecaoAtiva(secoes, limite));
    }

    // rAF em vez de chamada sincrona no efeito (evita cascata de render e o lint).
    const raf = requestAnimationFrame(medir);
    window.addEventListener("scroll", medir, { passive: true });
    window.addEventListener("resize", medir);
    return () => {
      cancelAnimationFrame(raf);
      window.removeEventListener("scroll", medir);
      window.removeEventListener("resize", medir);
    };
  }, [ids]);

  return ativa;
}
