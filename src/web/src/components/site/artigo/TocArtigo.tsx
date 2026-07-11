"use client";

import { useEffect, useState } from "react";
import { Idioma } from "@/components/Idioma";
import type { TituloArtigo } from "@/lib/artigos";

/** Indice lateral do artigo com scrollspy (destaca a secao visivel). */
export function TocArtigo({ titulos }: { titulos: readonly TituloArtigo[] }) {
  const [ativo, setAtivo] = useState<string | null>(titulos[0]?.id ?? null);

  useEffect(() => {
    const observador = new IntersectionObserver(
      (entradas) => {
        for (const entrada of entradas) {
          if (entrada.isIntersecting) {
            setAtivo(entrada.target.id);
          }
        }
      },
      { rootMargin: "-12% 0px -75% 0px" },
    );
    for (const titulo of titulos) {
      const el = document.getElementById(titulo.id);
      if (el) {
        observador.observe(el);
      }
    }
    return () => observador.disconnect();
  }, [titulos]);

  if (titulos.length === 0) {
    return null;
  }

  return (
    <aside className="sticky top-[92px] hidden border-l border-site-line pl-[18px] font-site-mono text-[12.5px] lg:block">
      <div className="mb-3.5 text-[10.5px] uppercase tracking-[0.1em] text-site-faint">
        <Idioma pt="Neste artigo" en="In this article" />
      </div>
      {titulos.map((titulo) => (
        <a
          key={titulo.id}
          href={`#${titulo.id}`}
          className={`block py-1.5 leading-tight transition-colors ${
            ativo === titulo.id ? "text-site-acc" : "text-site-fg2 hover:text-site-acc"
          }`}
        >
          {titulo.texto}
        </a>
      ))}
    </aside>
  );
}
