/**
 * Skeleton da home (site). Exibido durante a navegacao client-side enquanto a
 * pagina ISR e resolvida. Usa o design system site-*.
 */
export default function HomeLoading() {
  return (
    <div className="mx-auto flex min-h-screen max-w-4xl flex-col gap-16 px-6 py-16">
      {/* Hero skeleton */}
      <div className="flex flex-col gap-6">
        <div className="h-5 w-24 animate-pulse rounded bg-site-fg/10" />
        <div className="h-10 w-3/4 animate-pulse rounded bg-site-fg/10" />
        <div className="h-10 w-1/2 animate-pulse rounded bg-site-fg/10" />
        <div className="mt-4 h-4 w-2/3 animate-pulse rounded bg-site-fg/5" />
        <div className="h-4 w-1/2 animate-pulse rounded bg-site-fg/5" />
      </div>

      {/* Artigos skeleton */}
      <div className="flex flex-col gap-8">
        <div className="h-4 w-20 animate-pulse rounded bg-site-fg/10" />
        <div className="border-y border-t-site-line2 border-b-site-line py-8">
          <div className="mb-4 h-3 w-16 animate-pulse rounded bg-site-fg/10" />
          <div className="mb-3 h-8 w-3/4 animate-pulse rounded bg-site-fg/10" />
          <div className="h-4 w-1/2 animate-pulse rounded bg-site-fg/5" />
        </div>
        {[1, 2, 3].map((i) => (
          <div key={i} className="flex items-start gap-4 border-b border-site-line py-5">
            <div className="h-4 w-6 animate-pulse rounded bg-site-fg/10" />
            <div className="flex flex-1 flex-col gap-2">
              <div className="h-5 w-1/2 animate-pulse rounded bg-site-fg/10" />
              <div className="h-3 w-1/3 animate-pulse rounded bg-site-fg/5" />
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}
