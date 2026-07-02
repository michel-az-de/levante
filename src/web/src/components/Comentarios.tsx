"use client";

import { useEffect, useState } from "react";
import type { Comentario } from "@/types/domain";
import { FormComentario } from "@/components/FormComentario";

/**
 * Secao de comentarios do artigo: lista os aprovados (via BFF publico) e o
 * formulario de envio. O texto e renderizado como texto puro (React escapa;
 * sem HTML cru), evitando XSS.
 */
export function Comentarios({ artigoId, artigoSlug }: { artigoId: string; artigoSlug: string }) {
  const [comentarios, setComentarios] = useState<Comentario[] | null>(null);

  useEffect(() => {
    let ativo = true;
    fetch(`/api/publico/artigos/${artigoId}/comentarios`, { cache: "no-store" })
      .then(async (resposta) => {
        if (ativo) {
          setComentarios(resposta.ok ? ((await resposta.json()) as Comentario[]) : []);
        }
      })
      .catch(() => {
        if (ativo) {
          setComentarios([]);
        }
      });

    return () => {
      ativo = false;
    };
  }, [artigoId]);

  return (
    <section
      aria-label="Comentarios"
      className="flex flex-col gap-4 border-t border-neutral-200 pt-6 dark:border-neutral-800"
    >
      <h2 className="text-xl font-semibold tracking-tight">Comentarios</h2>

      {comentarios && comentarios.length > 0 ? (
        <ul className="flex flex-col gap-4">
          {comentarios.map((comentario) => (
            <li key={comentario.id} className="flex flex-col gap-1">
              <div className="flex items-baseline gap-2">
                <span className="font-medium">{comentario.autor}</span>
                <span className="text-xs text-neutral-500">
                  {new Date(comentario.dataCriacao).toLocaleDateString("pt-BR")}
                </span>
              </div>
              <p className="whitespace-pre-wrap text-neutral-700 dark:text-neutral-300">{comentario.texto}</p>
            </li>
          ))}
        </ul>
      ) : comentarios ? (
        <p className="text-sm text-neutral-500">Seja o primeiro a comentar.</p>
      ) : null}

      <FormComentario artigoId={artigoId} artigoSlug={artigoSlug} />
    </section>
  );
}
