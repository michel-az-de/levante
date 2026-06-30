import { cache } from "react";
import type { Metadata } from "next";
import { notFound } from "next/navigation";
import { ArtigoList } from "@/components/ArtigoList";
import { JsonLd } from "@/components/JsonLd";
import { artigoApi } from "@/lib/api";
import { site } from "@/lib/site";
import type { Artigo, Categoria } from "@/types/domain";

export const dynamic = "force-dynamic";

// cache(): generateMetadata e o componente compartilham o mesmo fetch no request.
const obterCategoria = cache(
  async (slug: string): Promise<{ categoria: Categoria; artigos: Artigo[] } | null> => {
    const { data, response } = await artigoApi.GET("/categorias/{slug}/artigos", {
      params: { path: { slug } },
    });

    if (response.status === 404) {
      return null;
    }
    if (!response.ok) {
      throw new Error(`Falha ao listar a categoria '${slug}': HTTP ${response.status}`);
    }

    const { data: categorias } = await artigoApi.GET("/categorias");
    const categoria = categorias?.find((c) => c.slug === slug);
    if (!categoria) {
      return null;
    }

    return { categoria, artigos: data ?? [] };
  },
);

export async function generateMetadata({
  params,
}: {
  params: Promise<{ slug: string }>;
}): Promise<Metadata> {
  const { slug } = await params;
  const dados = await obterCategoria(slug);
  if (!dados) {
    return { title: "Categoria nao encontrada" };
  }

  const caminho = `/categoria/${dados.categoria.slug}`;
  const descricao = dados.categoria.descricao ?? `Artigos da categoria ${dados.categoria.nome}.`;
  return {
    title: `Categoria: ${dados.categoria.nome}`,
    description: descricao,
    alternates: { canonical: caminho },
    openGraph: { title: dados.categoria.nome, description: descricao, url: `${site.url}${caminho}` },
  };
}

export default async function CategoriaPage({
  params,
}: {
  params: Promise<{ slug: string }>;
}) {
  const { slug } = await params;
  const dados = await obterCategoria(slug);
  if (!dados) {
    notFound();
  }

  const url = `${site.url}/categoria/${dados.categoria.slug}`;
  const collection = {
    "@context": "https://schema.org",
    "@type": "CollectionPage",
    name: dados.categoria.nome,
    description: dados.categoria.descricao ?? undefined,
    url,
  };

  return (
    <main className="mx-auto flex min-h-screen max-w-3xl flex-col gap-6 px-6 py-16">
      <JsonLd data={collection} />
      <header className="flex flex-col gap-2">
        <p className="text-sm uppercase tracking-wide text-neutral-500">Categoria</p>
        <h1 className="text-3xl font-bold tracking-tight">{dados.categoria.nome}</h1>
        {dados.categoria.descricao ? (
          <p className="text-lg text-neutral-600 dark:text-neutral-400">{dados.categoria.descricao}</p>
        ) : null}
      </header>
      <ArtigoList artigos={dados.artigos} categorias={[dados.categoria]} />
    </main>
  );
}
