import type { Metadata } from "next";
import Link from "next/link";
import { JsonLd } from "@/components/JsonLd";
import { site } from "@/lib/site";

export const metadata: Metadata = {
  alternates: { canonical: "/" },
  openGraph: { title: site.nome, description: site.descricao, url: site.url },
};

const pessoa = {
  "@context": "https://schema.org",
  "@type": "Person",
  name: site.autor,
  jobTitle: site.jobTitle,
  url: site.url,
  sameAs: site.sameAs,
  knowsAbout: site.knowsAbout,
};

const website = {
  "@context": "https://schema.org",
  "@type": "WebSite",
  name: site.nome,
  url: site.url,
};

export default function HomePage() {
  return (
    <main className="mx-auto flex min-h-screen max-w-3xl flex-col justify-center gap-6 px-6">
      <JsonLd data={pessoa} />
      <JsonLd data={website} />
      <h1 className="text-4xl font-bold tracking-tight">{site.nome}</h1>
      <p className="text-lg text-neutral-600 dark:text-neutral-400">
        Da pedra bruta a pedra polida. Blog tecnico, publicacoes e portfolio de {site.autor}.
      </p>
      <Link
        href="/artigos"
        className="w-fit rounded-md bg-neutral-900 px-4 py-2 text-white transition hover:bg-neutral-700 dark:bg-white dark:text-neutral-900 dark:hover:bg-neutral-200"
      >
        Ver artigos
      </Link>
    </main>
  );
}
