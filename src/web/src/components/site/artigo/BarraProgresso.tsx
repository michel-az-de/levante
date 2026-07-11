"use client";

import { useEffect, useRef } from "react";

/** Barra de progresso de leitura no topo. Escreve --progresso direto no DOM. */
export function BarraProgresso() {
  const ref = useRef<HTMLDivElement>(null);

  useEffect(() => {
    const el = ref.current;
    if (!el) {
      return;
    }
    const alvo = el;
    let raf = 0;

    function atualizar() {
      const doc = document.documentElement;
      const max = doc.scrollHeight - doc.clientHeight;
      const razao = max > 0 ? Math.min(1, Math.max(0, doc.scrollTop / max)) : 0;
      alvo.style.setProperty("--progresso", String(razao));
    }
    function aoScroll() {
      cancelAnimationFrame(raf);
      raf = requestAnimationFrame(atualizar);
    }

    atualizar();
    window.addEventListener("scroll", aoScroll, { passive: true });
    window.addEventListener("resize", aoScroll);
    return () => {
      cancelAnimationFrame(raf);
      window.removeEventListener("scroll", aoScroll);
      window.removeEventListener("resize", aoScroll);
    };
  }, []);

  return <div ref={ref} className="site-progresso" aria-hidden="true" />;
}
