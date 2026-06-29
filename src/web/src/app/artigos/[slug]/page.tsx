import type { Metadata } from "next";
import { notFound } from "next/navigation";
import { JsonLd } from "@/components/JsonLd";
import { artigoApi } from "@/lib/api";
import { site } from "@/lib/site";
import type { Artigo } from "@/types/domain";

export const revalidate = 300;

async function obterArtigo(slug: string): Promise<Artigo | null> {
  const { data, error } = await artigoApi.GET("/artigos/{slug}", {
    params: { path: { slug } },
  });
  return error || !data ? null : data;
}

export async function generateMetadata({
  params,
}: {
  params: Promise<{ slug: string }>;
}): Promise<Metadata> {
  const { slug } = await params;
  const artigo = await obterArtigo(slug);
  if (!artigo) {
    return { title: "Artigo nao encontrado" };
  }

  const caminho = `/artigos/${artigo.slug}`;
  return {
    title: artigo.titulo,
    description: artigo.resumo,
    alternates: { canonical: caminho },
    openGraph: {
      title: artigo.titulo,
      description: artigo.resumo,
      url: `${site.url}${caminho}`,
      type: "article",
      publishedTime: artigo.dataPublicacao ?? undefined,
      authors: [site.autor],
    },
    twitter: { card: "summary_large_image", title: artigo.titulo, description: artigo.resumo },
  };
}

export default async function ArtigoPage({
  params,
}: {
  params: Promise<{ slug: string }>;
}) {
  const { slug } = await params;
  const artigo = await obterArtigo(slug);
  if (!artigo) {
    notFound();
  }

  const url = `${site.url}/artigos/${artigo.slug}`;
  const dataPublicacao = artigo.dataPublicacao
    ? new Date(artigo.dataPublicacao).toLocaleDateString("pt-BR")
    : null;

  const blogPosting = {
    "@context": "https://schema.org",
    "@type": "BlogPosting",
    headline: artigo.titulo,
    description: artigo.resumo,
    datePublished: artigo.dataPublicacao ?? undefined,
    author: { "@type": "Person", name: site.autor, url: site.url },
    mainEntityOfPage: url,
    url,
  };

  const breadcrumb = {
    "@context": "https://schema.org",
    "@type": "BreadcrumbList",
    itemListElement: [
      { "@type": "ListItem", position: 1, name: "Artigos", item: `${site.url}/artigos` },
      { "@type": "ListItem", position: 2, name: artigo.titulo, item: url },
    ],
  };

  return (
    <main className="mx-auto flex min-h-screen max-w-3xl flex-col gap-4 px-6 py-16">
      <JsonLd data={blogPosting} />
      <JsonLd data={breadcrumb} />
      <article className="flex flex-col gap-4">
        <header className="flex flex-col gap-2">
          <h1 className="text-3xl font-bold tracking-tight">{artigo.titulo}</h1>
          {dataPublicacao ? (
            <p className="text-sm text-neutral-500">Publicado em {dataPublicacao}</p>
          ) : null}
          <p className="text-lg text-neutral-600 dark:text-neutral-400">{artigo.resumo}</p>
        </header>
        <div className="leading-relaxed text-neutral-800 dark:text-neutral-200">
          {artigo.conteudo}
        </div>
      </article>
    </main>
  );
}
