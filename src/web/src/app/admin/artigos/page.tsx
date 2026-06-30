"use client";

import Link from "next/link";
import { useRouter } from "next/navigation";
import { useCallback, useEffect, useState } from "react";
import { useGuardaAdmin, tratarNaoAutorizado } from "@/lib/admin-guard";
import { apiAdmin } from "@/lib/auth";
import type { Artigo } from "@/types/domain";

const corDoStatus: Record<string, string> = {
  Rascunho: "bg-neutral-200 text-neutral-700 dark:bg-neutral-800 dark:text-neutral-300",
  Publicado: "bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-200",
  Arquivado: "bg-amber-100 text-amber-800 dark:bg-amber-900 dark:text-amber-200",
};

export default function AdminArtigosPage() {
  const router = useRouter();
  const autorizado = useGuardaAdmin();
  const [artigos, setArtigos] = useState<Artigo[] | null>(null);
  const [ocupado, setOcupado] = useState(false);

  const carregar = useCallback(() => {
    // setState dentro do callback assincrono (nao sincrono no efeito).
    void apiAdmin.GET("/admin/artigos").then(({ data, response }) => {
      if (tratarNaoAutorizado(response.status, router)) {
        return;
      }
      setArtigos(data ?? []);
    });
  }, [router]);

  useEffect(() => {
    if (autorizado) {
      carregar();
    }
  }, [autorizado, carregar]);

  async function publicar(id: string) {
    setOcupado(true);
    const { response } = await apiAdmin.POST("/artigos/{id}/publicar", { params: { path: { id } } });
    setOcupado(false);
    if (!tratarNaoAutorizado(response.status, router)) {
      await carregar();
    }
  }

  async function arquivar(id: string) {
    setOcupado(true);
    const { response } = await apiAdmin.POST("/artigos/{id}/arquivar", { params: { path: { id } } });
    setOcupado(false);
    if (!tratarNaoAutorizado(response.status, router)) {
      await carregar();
    }
  }

  if (!autorizado || artigos === null) {
    return (
      <main className="mx-auto flex min-h-screen max-w-3xl items-center justify-center px-6">
        <p className="text-neutral-500">Carregando...</p>
      </main>
    );
  }

  return (
    <main className="mx-auto flex min-h-screen max-w-4xl flex-col gap-6 px-6 py-16">
      <div className="flex items-center justify-between">
        <h1 className="text-3xl font-bold tracking-tight">Artigos</h1>
        <Link
          href="/admin/artigos/novo"
          className="rounded-md bg-neutral-900 px-4 py-2 text-sm text-white transition hover:bg-neutral-700 dark:bg-white dark:text-neutral-900 dark:hover:bg-neutral-200"
        >
          Novo artigo
        </Link>
      </div>

      {artigos.length === 0 ? (
        <p className="text-neutral-500">Nenhum artigo ainda. Crie o primeiro.</p>
      ) : (
        <ul className="flex flex-col divide-y divide-neutral-200 dark:divide-neutral-800">
          {artigos.map((artigo) => (
            <li key={artigo.id} className="flex items-center justify-between gap-4 py-3">
              <div className="flex flex-col gap-1">
                <span className="font-medium">{artigo.titulo}</span>
                <span className="font-mono text-xs text-neutral-500">{artigo.slug}</span>
              </div>
              <div className="flex items-center gap-2">
                <span
                  className={`rounded-full px-2 py-0.5 text-xs ${corDoStatus[artigo.status] ?? corDoStatus.Rascunho}`}
                >
                  {artigo.status}
                </span>
                {artigo.status !== "Arquivado" ? (
                  <>
                    <Link
                      href={`/admin/artigos/${artigo.id}/editar`}
                      className="rounded border border-neutral-300 px-2 py-1 text-xs transition hover:bg-neutral-100 dark:border-neutral-700 dark:hover:bg-neutral-800"
                    >
                      Editar
                    </Link>
                    {artigo.status === "Rascunho" ? (
                      <button
                        type="button"
                        disabled={ocupado}
                        onClick={() => publicar(artigo.id)}
                        className="rounded border border-neutral-300 px-2 py-1 text-xs transition hover:bg-neutral-100 disabled:opacity-50 dark:border-neutral-700 dark:hover:bg-neutral-800"
                      >
                        Publicar
                      </button>
                    ) : null}
                    <button
                      type="button"
                      disabled={ocupado}
                      onClick={() => arquivar(artigo.id)}
                      className="rounded border border-neutral-300 px-2 py-1 text-xs transition hover:bg-neutral-100 disabled:opacity-50 dark:border-neutral-700 dark:hover:bg-neutral-800"
                    >
                      Arquivar
                    </button>
                  </>
                ) : null}
              </div>
            </li>
          ))}
        </ul>
      )}
    </main>
  );
}
