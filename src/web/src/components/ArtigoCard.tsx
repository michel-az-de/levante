import type { Artigo } from "@/types/domain";

export function ArtigoCard({ artigo }: { artigo: Artigo }) {
  const data = artigo.dataPublicacao
    ? new Date(artigo.dataPublicacao).toLocaleDateString("pt-BR")
    : null;

  return (
    <article className="rounded-lg border border-neutral-200 p-5 transition hover:shadow-md dark:border-neutral-800">
      <h2 className="text-xl font-semibold">{artigo.titulo}</h2>
      {data ? (
        <p className="mt-1 text-sm text-neutral-500">Publicado em {data}</p>
      ) : null}
      <p className="mt-3 text-neutral-700 dark:text-neutral-300">{artigo.conteudo}</p>
      <p className="mt-3 font-mono text-xs text-neutral-400">/{artigo.slug}</p>
    </article>
  );
}
