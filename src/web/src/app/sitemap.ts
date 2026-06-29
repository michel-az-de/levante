import type { MetadataRoute } from "next";
import { artigoApi } from "@/lib/api";
import { site } from "@/lib/site";
import type { Artigo } from "@/types/domain";

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

export default async function sitemap(): Promise<MetadataRoute.Sitemap> {
  const artigos = await listarArtigos();

  return [
    { url: `${site.url}/`, changeFrequency: "weekly", priority: 1 },
    { url: `${site.url}/artigos`, changeFrequency: "daily", priority: 0.8 },
    ...artigos.map((artigo) => ({
      url: `${site.url}/artigos/${artigo.slug}`,
      lastModified: artigo.dataPublicacao ?? undefined,
      changeFrequency: "monthly" as const,
      priority: 0.7,
    })),
  ];
}
