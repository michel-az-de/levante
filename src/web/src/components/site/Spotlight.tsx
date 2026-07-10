"use client";

import { useEffect, useRef } from "react";

/**
 * Luz que segue o cursor (atras do conteudo). Escreve --spot-x/--spot-y direto
 * no DOM via rAF, sem setState (nao re-renderiza). CSS cuida do reduced-motion.
 */
export function Spotlight() {
  const ref = useRef<HTMLDivElement>(null);

  useEffect(() => {
    const el = ref.current;
    if (!el) {
      return;
    }
    // Const nao-nulo para preservar o tipo dentro dos closures do rAF.
    const alvo = el;
    const fino = window.matchMedia("(pointer: fine)").matches;
    const reduz = window.matchMedia("(prefers-reduced-motion: reduce)").matches;
    if (!fino || reduz) {
      return;
    }

    let destinoX = window.innerWidth / 2;
    let destinoY = window.innerHeight / 2;
    let x = destinoX;
    let y = destinoY;
    let ativo = false;
    let raf = 0;

    function mover(evento: PointerEvent) {
      destinoX = evento.clientX;
      destinoY = evento.clientY;
      if (!ativo) {
        ativo = true;
        alvo.dataset.ativo = "true";
      }
    }
    function loop() {
      x += (destinoX - x) * 0.12;
      y += (destinoY - y) * 0.12;
      alvo.style.setProperty("--spot-x", `${x}px`);
      alvo.style.setProperty("--spot-y", `${y}px`);
      raf = requestAnimationFrame(loop);
    }

    window.addEventListener("pointermove", mover);
    raf = requestAnimationFrame(loop);
    return () => {
      window.removeEventListener("pointermove", mover);
      cancelAnimationFrame(raf);
    };
  }, []);

  return <div ref={ref} className="site-spot" aria-hidden="true" />;
}
