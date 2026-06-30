import type { MetadataRoute } from "next";
import { artigoApi } from "@/lib/api";
import { site } from "@/lib/site";
import type { Artigo, Categoria } from "@/types/domain";

export const revalidate = 3600;

async function listarArtigos(): Promise<Artigo[]> {
  try {
    const { data } = await artigoApi.GET("/artigos");
    return data ?? [];
  } catch {
    // API fora no build: o sitemap ainda sai com as rotas estaticas.
    return [];
  }
}

async function listarCategorias(): Promise<Categoria[]> {
  try {
    const { data } = await artigoApi.GET("/categorias");
    return data ?? [];
  } catch {
    return [];
  }
}

export default async function sitemap(): Promise<MetadataRoute.Sitemap> {
  const [artigos, categorias] = await Promise.all([listarArtigos(), listarCategorias()]);

  return [
    { url: `${site.url}/`, changeFrequency: "weekly", priority: 1 },
    { url: `${site.url}/artigos`, changeFrequency: "daily", priority: 0.8 },
    ...categorias.map((categoria) => ({
      url: `${site.url}/categoria/${categoria.slug}`,
      changeFrequency: "weekly" as const,
      priority: 0.6,
    })),
    ...artigos.map((artigo) => ({
      url: `${site.url}/artigos/${artigo.slug}`,
      lastModified: artigo.dataPublicacao ?? undefined,
      changeFrequency: "monthly" as const,
      priority: 0.7,
    })),
  ];
}
