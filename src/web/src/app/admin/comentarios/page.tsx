"use client";

import Link from "next/link";
import { useRouter } from "next/navigation";
import { useCallback, useEffect, useState } from "react";
import { useGuardaAdmin, tratarNaoAutorizado } from "@/lib/admin-guard";
import { apiAdmin } from "@/lib/auth";
import type { Comentario } from "@/types/domain";

/** Fila de moderacao: comentarios pendentes; aprovar/rejeitar via BFF do admin. */
export default function AdminComentariosPage() {
  const router = useRouter();
  const autorizado = useGuardaAdmin();
  const [comentarios, setComentarios] = useState<Comentario[] | null>(null);
  const [ocupado, setOcupado] = useState(false);
  const [erro, setErro] = useState<string | null>(null);

  const carregar = useCallback(() => {
    apiAdmin
      .GET("/admin/comentarios")
      .then(({ data, response }) => {
        if (tratarNaoAutorizado(response.status, router)) {
          return;
        }
        if (!response.ok) {
          setErro("Falha ao carregar a fila de moderacao.");
          setComentarios([]);
          return;
        }
        setErro(null);
        setComentarios(data ?? []);
      })
      .catch(() => {
        setErro("Falha de conexao ao carregar os comentarios.");
        setComentarios([]);
      });
  }, [router]);

  useEffect(() => {
    if (autorizado) {
      carregar();
    }
  }, [autorizado, carregar]);

  async function moderar(operacao: Promise<{ response: Response }>) {
    setErro(null);
    setOcupado(true);
    try {
      const { response } = await operacao;
      if (tratarNaoAutorizado(response.status, router)) {
        return;
      }
      if (!response.ok) {
        setErro("Falha ao moderar o comentario.");
        return;
      }
      carregar();
    } catch {
      setErro("Falha de conexao. Tente novamente.");
    } finally {
      setOcupado(false);
    }
  }

  function aprovar(id: string) {
    void moderar(apiAdmin.POST("/admin/comentarios/{id}/aprovar", { params: { path: { id } } }));
  }

  function rejeitar(id: string) {
    void moderar(apiAdmin.POST("/admin/comentarios/{id}/rejeitar", { params: { path: { id } } }));
  }

  if (!autorizado || comentarios === null) {
    return (
      <main className="mx-auto flex min-h-screen max-w-3xl items-center justify-center px-6">
        <p className="text-neutral-500">Carregando...</p>
      </main>
    );
  }

  return (
    <main className="mx-auto flex min-h-screen max-w-3xl flex-col gap-6 px-6 py-16">
      <div className="flex items-center justify-between">
        <h1 className="text-3xl font-bold tracking-tight">Moderacao de comentarios</h1>
        <Link href="/admin" className="text-sm text-neutral-500 hover:underline">
          Voltar
        </Link>
      </div>

      {erro ? <p className="text-sm text-red-600">{erro}</p> : null}

      {comentarios.length === 0 ? (
        <p className="text-neutral-500">Nenhum comentario pendente.</p>
      ) : (
        <ul className="flex flex-col divide-y divide-neutral-200 dark:divide-neutral-800">
          {comentarios.map((comentario) => (
            <li key={comentario.id} className="flex flex-col gap-2 py-4">
              <div className="flex items-baseline gap-2">
                <span className="font-medium">{comentario.autor}</span>
                <Link
                  href={`/artigos/${comentario.artigoSlug}`}
                  className="text-xs text-blue-600 hover:underline dark:text-blue-400"
                >
                  /artigos/{comentario.artigoSlug}
                </Link>
              </div>
              <p className="whitespace-pre-wrap text-neutral-700 dark:text-neutral-300">{comentario.texto}</p>
              <div className="flex gap-2">
                <button
                  type="button"
                  disabled={ocupado}
                  onClick={() => aprovar(comentario.id)}
                  className="rounded border border-neutral-300 px-2 py-1 text-xs transition hover:bg-neutral-100 disabled:opacity-50 dark:border-neutral-700 dark:hover:bg-neutral-800"
                >
                  Aprovar
                </button>
                <button
                  type="button"
                  disabled={ocupado}
                  onClick={() => rejeitar(comentario.id)}
                  className="rounded border border-neutral-300 px-2 py-1 text-xs transition hover:bg-neutral-100 disabled:opacity-50 dark:border-neutral-700 dark:hover:bg-neutral-800"
                >
                  Rejeitar
                </button>
              </div>
            </li>
          ))}
        </ul>
      )}
    </main>
  );
}
