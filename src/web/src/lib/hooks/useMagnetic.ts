"use client";

import { useEffect, useRef } from "react";

/**
 * Efeito magnetico: o elemento acompanha levemente o cursor no hover. Aplicado
 * por ref. So ativa em ponteiro fino e sem prefers-reduced-motion; escreve o
 * transform direto no DOM (sem setState) para nao re-renderizar a cada frame.
 */
export function useMagnetic<T extends HTMLElement>() {
  const ref = useRef<T>(null);

  useEffect(() => {
    const el = ref.current;
    if (!el) {
      return;
    }
    const fino = window.matchMedia("(pointer: fine)").matches;
    const reduz = window.matchMedia("(prefers-reduced-motion: reduce)").matches;
    if (!fino || reduz) {
      return;
    }

    function mover(evento: PointerEvent) {
      const alvo = el as T;
      const r = alvo.getBoundingClientRect();
      const dx = evento.clientX - (r.left + r.width / 2);
      const dy = evento.clientY - (r.top + r.height / 2);
      alvo.style.transform = `translate(${dx * 0.22}px, ${dy * 0.3}px)`;
    }
    function sair() {
      (el as T).style.transform = "";
    }

    el.addEventListener("pointermove", mover);
    el.addEventListener("pointerleave", sair);
    return () => {
      el.removeEventListener("pointermove", mover);
      el.removeEventListener("pointerleave", sair);
    };
  }, []);

  return ref;
}
