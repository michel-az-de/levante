import Link from "next/link";
import { Idioma } from "@/components/Idioma";

/**
 * 404 do grupo (site): herda o chrome (header/footer/tema) — cobre o notFound()
 * de artigo/categoria. URLs fora do grupo continuam caindo no app/not-found.tsx.
 */
export default function NotFoundSite() {
  return (
    <div className="mx-auto flex min-h-[60vh] max-w-2xl flex-col justify-center gap-4 px-6 py-16">
      <p className="font-site-mono text-xs uppercase tracking-wider text-site-faint">404</p>
      <h1 className="text-3xl font-bold tracking-tight">
        <Idioma pt="Página não encontrada." en="Page not found." />
      </h1>
      <p className="text-site-fg2">
        <Idioma
          pt="O endereço pode ter mudado ou nunca existiu."
          en="The address may have changed or never existed."
        />
      </p>
      <div className="flex gap-5">
        <Link href="/" className="w-fit underline transition-colors hover:text-site-acc">
          <Idioma pt="Voltar ao início" en="Back home" />
        </Link>
        <Link href="/artigos" className="w-fit underline transition-colors hover:text-site-acc">
          <Idioma pt="Ver artigos" en="Browse articles" />
        </Link>
      </div>
    </div>
  );
}
