"use client";

import { useCallback, useEffect, useRef, useState } from "react";
import type { Comentario } from "@/types/domain";
import { FormComentario } from "@/components/FormComentario";

/**
 * Secao de comentarios do artigo: lista os aprovados (via BFF publico) e o
 * formulario de envio. O texto e renderizado como texto puro (React escapa;
 * sem HTML cru), evitando XSS.
 */
export function Comentarios({ artigoId, artigoSlug }: { artigoId: string; artigoSlug: string }) {
  const [comentarios, setComentarios] = useState<Comentario[] | null>(null);
  const [erro, setErro] = useState<string | null>(null);
  const abortRef = useRef<AbortController | null>(null);

  const carregarComentarios = useCallback(() => {
    // Aborta a requisicao anterior (retry/StrictMode/remount): so a mais recente
    // escreve estado — sem isso uma resposta lenta antiga apagaria a lista boa.
    abortRef.current?.abort();
    const controlador = new AbortController();
    abortRef.current = controlador;

    fetch(`/api/publico/artigos/${artigoId}/comentarios`, {
      cache: "no-store",
      signal: controlador.signal,
    })
      .then(async (resposta) => {
        const dados = resposta.ok ? ((await resposta.json()) as Comentario[]) : null;
        if (controlador.signal.aborted) {
          return;
        }
        if (dados) {
          setComentarios(dados);
          setErro(null);
        } else {
          // Falha nao destroi lista ja carregada; so exibe o alerta.
          setErro("Nao foi possivel carregar os comentarios.");
        }
      })
      .catch(() => {
        if (!controlador.signal.aborted) {
          setErro("Erro de conexao. Tente novamente.");
        }
      });
  }, [artigoId]);

  function tentarNovamente() {
    setErro(null);
    carregarComentarios();
  }

  useEffect(() => {
    carregarComentarios();
    return () => abortRef.current?.abort();
  }, [carregarComentarios]);

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
      ) : comentarios && !erro ? (
        <p className="text-sm text-neutral-500">Seja o primeiro a comentar.</p>
      ) : null}

      {erro && (
        <p className="text-sm text-red-600 dark:text-red-400" role="alert">
          {erro}{" "}
          <button
            type="button"
            className="underline"
            onClick={tentarNovamente}
          >
            Tentar novamente
          </button>
        </p>
      )}

      <FormComentario artigoId={artigoId} artigoSlug={artigoSlug} />
    </section>
  );
}
