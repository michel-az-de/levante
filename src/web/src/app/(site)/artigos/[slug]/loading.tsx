/**
 * Skeleton do artigo (site). Exibido enquanto o ISR revalida ou carrega um slug.
 * Espelha o layout de artigo: titulo, meta, conteudo em paragrafos.
 */
export default function ArtigoLoading() {
  return (
    <article className="mx-auto min-h-screen max-w-2xl px-6 pb-20 pt-16">
      {/* Tags */}
      <div className="mb-4 flex gap-2">
        <div className="h-5 w-16 animate-pulse rounded bg-site-fg/10" />
        <div className="h-5 w-20 animate-pulse rounded bg-site-fg/10" />
      </div>

      {/* Titulo */}
      <div className="mb-3 h-10 w-full animate-pulse rounded bg-site-fg/10" />
      <div className="mb-6 h-10 w-3/4 animate-pulse rounded bg-site-fg/10" />

      {/* Meta (data, tempo, categoria) */}
      <div className="mb-8 flex gap-3">
        <div className="h-3 w-24 animate-pulse rounded bg-site-fg/5" />
        <div className="h-3 w-12 animate-pulse rounded bg-site-fg/5" />
        <div className="h-3 w-16 animate-pulse rounded bg-site-fg/5" />
      </div>

      {/* Paragrafos do conteudo */}
      <div className="flex flex-col gap-4">
        <div className="h-4 w-full animate-pulse rounded bg-site-fg/5" />
        <div className="h-4 w-full animate-pulse rounded bg-site-fg/5" />
        <div className="h-4 w-5/6 animate-pulse rounded bg-site-fg/5" />
        <div className="h-4 w-full animate-pulse rounded bg-site-fg/5" />
        <div className="h-4 w-3/4 animate-pulse rounded bg-site-fg/5" />
        <div className="h-4 w-full animate-pulse rounded bg-site-fg/5" />
        <div className="h-4 w-5/6 animate-pulse rounded bg-site-fg/5" />
        <div className="h-4 w-full animate-pulse rounded bg-site-fg/5" />
        <div className="h-4 w-2/3 animate-pulse rounded bg-site-fg/5" />
      </div>
    </article>
  );
}
