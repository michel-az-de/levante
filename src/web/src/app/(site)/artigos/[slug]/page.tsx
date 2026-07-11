import { cache } from "react";
import type { Metadata } from "next";
import Link from "next/link";
import { notFound } from "next/navigation";
import { Comentarios } from "@/components/Comentarios";
import { Idioma } from "@/components/Idioma";
import { JsonLd } from "@/components/JsonLd";
import { Markdown } from "@/components/Markdown";
import { ReacoesArtigo } from "@/components/ReacoesArtigo";
import { BarraProgresso } from "@/components/site/artigo/BarraProgresso";
import { TocArtigo } from "@/components/site/artigo/TocArtigo";
import { artigoApi } from "@/lib/api";
import { extrairTitulos, formatarData, tempoLeitura } from "@/lib/artigos";
import { site } from "@/lib/site";
import type { Artigo, Categoria } from "@/types/domain";

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

// Resolve a categoria do artigo para exibir nome/link (front compoe o read model).
const obterCategorias = cache(async (): Promise<Categoria[]> => {
  const { data } = await artigoApi.GET("/categorias");
  return data ?? [];
});

// Lista publicada para navegar anterior/proximo. Falha vira lista vazia (o
// prev/next some, mas o artigo continua legivel).
const obterLista = cache(async (): Promise<Artigo[]> => {
  try {
    const { data } = await artigoApi.GET("/artigos");
    return data ?? [];
  } catch {
    return [];
  }
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

function ProximoLink({
  artigo,
  tipo,
}: {
  artigo: Artigo;
  tipo: "anterior" | "proximo";
}) {
  const anterior = tipo === "anterior";
  return (
    <Link
      href={`/artigos/${artigo.slug}`}
      className={`group block bg-site-bg p-6 transition-colors hover:bg-site-bg1 ${anterior ? "" : "text-right"}`}
    >
      <div className="mb-2.5 font-site-mono text-[11px] text-site-faint">
        {anterior ? (
          <Idioma pt="← anterior" en="← previous" />
        ) : (
          <Idioma pt="próximo →" en="next →" />
        )}
      </div>
      <div className="text-[17px] leading-tight font-semibold text-site-fg group-hover:text-site-acc">
        {artigo.titulo}
      </div>
    </Link>
  );
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

  const [categorias, lista] = await Promise.all([obterCategorias(), obterLista()]);
  const categoria = artigo.categoriaId
    ? categorias.find((c) => c.id === artigo.categoriaId)
    : undefined;

  const indice = lista.findIndex((a) => a.slug === artigo.slug);
  const anterior = indice > 0 ? lista[indice - 1] : undefined;
  const proximo = indice >= 0 && indice < lista.length - 1 ? lista[indice + 1] : undefined;

  const titulos = extrairTitulos(artigo.conteudo);
  const url = `${site.url}/artigos/${artigo.slug}`;

  const blogPosting = {
    "@context": "https://schema.org",
    "@type": "BlogPosting",
    headline: artigo.titulo,
    description: artigo.resumo,
    datePublished: artigo.dataPublicacao ?? undefined,
    author: { "@type": "Person", name: site.autor, url: site.url },
    articleSection: categoria?.nome,
    keywords: artigo.tags.length > 0 ? artigo.tags.join(", ") : undefined,
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

  const pill = categoria?.nome ?? artigo.tags[0];

  return (
    <div className="mx-auto max-w-[900px] px-[clamp(18px,4vw,40px)]">
      <BarraProgresso />
      <JsonLd data={blogPosting} />
      <JsonLd data={breadcrumb} />

      <Link
        href="/artigos"
        className="inline-flex items-center gap-2 pt-[clamp(34px,6vw,60px)] font-site-mono text-[12.5px] text-site-fg2 transition-colors hover:text-site-acc"
      >
        ← <Idioma pt="Todos os artigos" en="All articles" />
      </Link>

      <article>
        <header className="mb-10 border-b border-site-line pt-6 pb-[30px]">
          {pill ? (
            categoria ? (
              <Link href={`/categoria/${categoria.slug}`} className="site-pill mb-[18px] inline-block">
                {pill}
              </Link>
            ) : (
              <span className="site-pill mb-[18px] inline-block">{pill}</span>
            )
          ) : null}
          <h1 className="mb-5 max-w-[22ch] text-[clamp(30px,5vw,56px)] leading-[1.03] font-bold tracking-[-0.03em] text-site-fg">
            {artigo.titulo}
          </h1>
          <div className="flex flex-wrap gap-2.5 font-site-mono text-[12.5px] text-site-faint">
            <span>{formatarData(artigo.dataPublicacao)}</span>
            <span>·</span>
            <span>{tempoLeitura(artigo.conteudo)} min</span>
            <span>·</span>
            <span>
              <Idioma pt="por" en="by" /> {site.autor}
            </span>
          </div>
        </header>

        <div className="grid grid-cols-1 items-start gap-[54px] lg:grid-cols-[1fr_210px]">
          <div className="min-w-0">
            <Markdown>{artigo.conteudo}</Markdown>
            <ReacoesArtigo artigoId={artigo.id} />
          </div>
          <TocArtigo titulos={titulos} />
        </div>

        {anterior || proximo ? (
          <div className="mt-14 grid grid-cols-1 gap-px border border-site-line bg-site-line sm:grid-cols-2">
            {anterior ? <ProximoLink artigo={anterior} tipo="anterior" /> : <span className="bg-site-bg" />}
            {proximo ? <ProximoLink artigo={proximo} tipo="proximo" /> : <span className="bg-site-bg" />}
          </div>
        ) : null}

        <div className="mt-10 flex flex-wrap items-center justify-between gap-3.5 border-t border-site-line pt-6">
          <span className="font-site-mono text-xs text-site-faint">
            <Idioma pt="publicado no Levante" en="published on Levante" />
          </span>
          <Link
            href="/newsletter"
            className="font-site-mono text-[13px] text-site-fg2 transition-colors hover:text-site-acc"
          >
            <Idioma pt="Assinar a newsletter" en="Subscribe" /> →
          </Link>
        </div>

        <div className="mt-10">
          <Comentarios artigoId={artigo.id} artigoSlug={artigo.slug} />
        </div>
      </article>
    </div>
  );
}
