"use client";

import Link from "next/link";
import { useEffect } from "react";

/**
 * 500 gracioso do App Router: captura erros de runtime nos segmentos abaixo do
 * layout raiz. Auto-contido (nao herda o layout do grupo (site)), no molde do
 * not-found. O erro e logado no stdout do container -> coletado pelo Loki (D1).
 */
export default function Error({
  error,
  reset,
}: {
  error: Error & { digest?: string };
  reset: () => void;
}) {
  useEffect(() => {
    console.error(error);
  }, [error]);

  return (
    <main className="mx-auto flex min-h-screen max-w-3xl flex-col justify-center gap-4 px-6">
      <h1 className="text-4xl font-bold tracking-tight">Algo deu errado</h1>
      <p className="text-lg text-neutral-600 dark:text-neutral-400">
        Um erro inesperado aconteceu. Voce pode tentar de novo ou voltar ao inicio.
      </p>
      <div className="flex gap-5">
        <button
          type="button"
          onClick={reset}
          className="w-fit text-neutral-900 underline dark:text-neutral-100"
        >
          Tentar de novo
        </button>
        <Link href="/" className="w-fit text-neutral-900 underline dark:text-neutral-100">
          Voltar ao inicio
        </Link>
      </div>
    </main>
  );
}
