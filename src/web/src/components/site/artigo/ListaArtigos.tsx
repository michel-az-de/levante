"use client";

import { useMemo, useState } from "react";
import { Idioma } from "@/components/Idioma";
import { LinhaArtigo } from "@/components/site/LinhaArtigo";
import type { Artigo } from "@/types/domain";

/** Lista de artigos com filtro por tag (chips). Usada em /artigos e /categoria. */
export function ListaArtigos({ artigos }: { artigos: Artigo[] }) {
  const tags = useMemo(() => {
    const conjunto = new Set<string>();
    for (const artigo of artigos) {
      for (const tag of artigo.tags) {
        conjunto.add(tag);
      }
    }
    return ["todos", ...Array.from(conjunto)];
  }, [artigos]);

  const [ativa, setAtiva] = useState("todos");
  const filtrados = ativa === "todos" ? artigos : artigos.filter((a) => a.tags.includes(ativa));

  return (
    <div>
      {tags.length > 1 ? (
        <div className="mb-2 flex flex-wrap gap-2">
          {tags.map((tag) => (
            <button
              key={tag}
              type="button"
              onClick={() => setAtiva(tag)}
              className={`border px-3 py-[7px] font-site-mono text-xs transition-colors ${
                tag === ativa
                  ? "border-site-acc bg-site-acc text-site-onacc"
                  : "border-site-line2 text-site-fg2 hover:border-site-acc hover:text-site-acc"
              }`}
            >
              {tag}
            </button>
          ))}
        </div>
      ) : null}

      <div>
        {filtrados.length === 0 ? (
          <p className="py-6 text-site-faint">
            <Idioma pt="Nada por aqui ainda." en="Nothing here yet." />
          </p>
        ) : (
          filtrados.map((artigo, i) => (
            <LinhaArtigo key={artigo.slug} artigo={artigo} numero={i + 1} />
          ))
        )}
      </div>
    </div>
  );
}
