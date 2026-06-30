import { cache } from "react";
import type { Metadata } from "next";
import { notFound } from "next/navigation";
import { JsonLd } from "@/components/JsonLd";
import { Markdown } from "@/components/Markdown";
import { artigoApi } from "@/lib/api";
import { site } from "@/lib/site";
import type { Artigo } from "@/types/domain";

export const revalidate = 300;

// cache(): generateMetadata e o componente compartilham o mesmo fetch no render
// do request, em vez de baterem na API duas vezes. (O opengraph-image roda em
// outro request e tem o seu proprio fetch, com fallback proprio.)
const obterArtigo = cache(async (slug: string): Promise<Artigo | null> => {
  const { data, response } = await artigoApi.GET("/artigos/{slug}", {
    params: { path: { slug } },
  });

  // 404 da API = artigo inexistente ou nao publicado -> notFound() (cacheavel).
  if (response.status === 404) {
    return null;
  }

  // Qualquer outra falha (API fora, 5xx) PROPAGA de proposito: com revalidate=300,
  // engolir o erro cacharia um 404 falso por 5 min para um artigo que existe.
  if (!response.ok) {
    throw new Error(`Falha ao obter artigo '${slug}': HTTP ${response.status}`);
  }

  return data ?? null;
});

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
  // Overrides de SEO editáveis (Fatia 2c-i); fallback para título/resumo do artigo.
  const titulo = artigo.metaTitulo ?? artigo.titulo;
  const descricao = artigo.metaDescricao ?? artigo.resumo;
  // imagemOgUrl definido sobrepõe o opengraph-image dinâmico (relativo resolve via metadataBase).
  const imagens = artigo.imagemOgUrl ? [artigo.imagemOgUrl] : undefined;

  return {
    title: titulo,
    description: descricao,
    alternates: { canonical: caminho },
    openGraph: {
      title: titulo,
      description: descricao,
      url: `${site.url}${caminho}`,
      type: "article",
      publishedTime: artigo.dataPublicacao ?? undefined,
      authors: [site.autor],
      ...(imagens ? { images: imagens } : {}),
    },
    twitter: {
      card: "summary_large_image",
      title: titulo,
      description: descricao,
      ...(imagens ? { images: imagens } : {}),
    },
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
        <Markdown>{artigo.conteudo}</Markdown>
      </article>
    </main>
  );
}
