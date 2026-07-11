import { cache } from "react";
import type { Metadata } from "next";
import { notFound } from "next/navigation";
import { Idioma } from "@/components/Idioma";
import { ListaArtigos } from "@/components/site/artigo/ListaArtigos";
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

    const { data: categorias, response: respostaCategorias } = await artigoApi.GET("/categorias");
    // Falha do fetch auxiliar != categoria inexistente: propaga em vez de virar 404 falso.
    if (!respostaCategorias.ok) {
      throw new Error(`Falha ao resolver a categoria '${slug}': HTTP ${respostaCategorias.status}`);
    }
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
    <div className="mx-auto max-w-[1180px] px-[clamp(18px,4vw,40px)] py-[clamp(48px,8vw,92px)]">
      <JsonLd data={collection} />
      <div className="site-label mb-3.5">
        <b className="font-medium text-site-acc">
          <Idioma pt="categoria" en="category" />
        </b>{" "}
        · Levante
      </div>
      <h1 className="mb-4 text-[clamp(36px,6vw,66px)] leading-none font-bold tracking-[-0.03em] text-site-fg">
        {dados.categoria.nome}
      </h1>
      {dados.categoria.descricao ? (
        <p className="mb-8 max-w-[54ch] text-base text-site-fg2">{dados.categoria.descricao}</p>
      ) : null}
      <ListaArtigos artigos={dados.artigos} />
    </div>
  );
}
