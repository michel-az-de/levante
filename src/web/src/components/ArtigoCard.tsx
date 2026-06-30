import Link from "next/link";
import type { Artigo } from "@/types/domain";

export function ArtigoCard({ artigo, categoriaNome }: { artigo: Artigo; categoriaNome?: string }) {
  const data = artigo.dataPublicacao
    ? new Date(artigo.dataPublicacao).toLocaleDateString("pt-BR")
    : null;

  return (
    <article className="rounded-lg border border-neutral-200 p-5 transition hover:shadow-md dark:border-neutral-800">
      {categoriaNome ? (
        <span className="text-xs font-medium uppercase tracking-wide text-blue-600 dark:text-blue-400">
          {categoriaNome}
        </span>
      ) : null}
      <h2 className="mt-1 text-xl font-semibold">
        <Link href={`/artigos/${artigo.slug}`} className="hover:underline">
          {artigo.titulo}
        </Link>
      </h2>
      {data ? (
        <p className="mt-1 text-sm text-neutral-500">Publicado em {data}</p>
      ) : null}
      <p className="mt-3 text-neutral-700 dark:text-neutral-300">{artigo.resumo}</p>
      {artigo.tags.length > 0 ? (
        <div className="mt-3 flex flex-wrap gap-1">
          {artigo.tags.map((tag) => (
            <span
              key={tag}
              className="rounded-full bg-neutral-100 px-2 py-0.5 text-xs text-neutral-600 dark:bg-neutral-800 dark:text-neutral-400"
            >
              #{tag}
            </span>
          ))}
        </div>
      ) : null}
    </article>
  );
}
