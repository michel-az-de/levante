"use client";

import { useEffect, useRef, type ReactNode } from "react";

/**
 * Envolve um bloco que entra suave ao aparecer na viewport (classe .vis via
 * IntersectionObserver). Sem setState — so alterna classe. Em reduced-motion,
 * ja nasce visivel.
 */
export function RevealOnScroll({
  children,
  className,
}: {
  children: ReactNode;
  className?: string;
}) {
  const ref = useRef<HTMLDivElement>(null);

  useEffect(() => {
    const el = ref.current;
    if (!el) {
      return;
    }
    if (window.matchMedia("(prefers-reduced-motion: reduce)").matches) {
      el.classList.add("vis");
      return;
    }
    const observador = new IntersectionObserver(
      (entradas) => {
        for (const entrada of entradas) {
          if (entrada.isIntersecting) {
            entrada.target.classList.add("vis");
            observador.unobserve(entrada.target);
          }
        }
      },
      { rootMargin: "-40px" },
    );
    observador.observe(el);
    return () => observador.disconnect();
  }, []);

  return (
    <div ref={ref} className={`site-reveal${className ? ` ${className}` : ""}`}>
      {children}
    </div>
  );
}
