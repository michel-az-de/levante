import type { MetadataRoute } from "next";
import { unstable_cache } from "next/cache";
import { artigoApi } from "@/lib/api";
import { siteIndexavel } from "@/lib/flags";
import { site } from "@/lib/site";
import type { Artigo, Categoria } from "@/types/domain";

// force-dynamic: le SITE_URL/flag em runtime (cutover = restart, nao rebuild).
export const dynamic = "force-dynamic";

// As buscas LANCAM em falha (sem catch aqui de proposito): o unstable_cache nao
// memoriza rejeicao, entao um blip da API nao congela um sitemap vazio por 1h.
// O fallback nao-cacheado fica nos wrappers *OuVazio abaixo.
async function listarArtigos(): Promise<Artigo[]> {
  const { data, response } = await artigoApi.GET("/artigos");
  if (!response.ok) {
    throw new Error(`Sitemap: /artigos respondeu HTTP ${response.status}`);
  }
  return data ?? [];
}

async function listarCategorias(): Promise<Categoria[]> {
  const { data, response } = await artigoApi.GET("/categorias");
  if (!response.ok) {
    throw new Error(`Sitemap: /categorias respondeu HTTP ${response.status}`);
  }
  return data ?? [];
}

// Dados cacheados (revalidate 1h): a rota fica force-dynamic para ler SITE_URL em runtime,
// mas nao refaz a chamada de API a cada request (custo sob crawler). Ver issue #84.
const artigosCacheados = unstable_cache(listarArtigos, ["sitemap:artigos"], { revalidate: 3600 });
const categoriasCacheadas = unstable_cache(listarCategorias, ["sitemap:categorias"], {
  revalidate: 3600,
});

async function artigosOuVazio(): Promise<Artigo[]> {
  try {
    return await artigosCacheados();
  } catch {
    // API fora: o sitemap ainda sai com as rotas estaticas.
    return [];
  }
}

async function categoriasOuVazio(): Promise<Categoria[]> {
  try {
    return await categoriasCacheadas();
  } catch {
    return [];
  }
}

export default async function sitemap(): Promise<MetadataRoute.Sitemap> {
  if (!siteIndexavel()) {
    // Host provisorio / pre-cutover: sitemap vazio (nao poluir o indice com URLs interinas).
    return [];
  }

  const [artigos, categorias] = await Promise.all([artigosOuVazio(), categoriasOuVazio()]);

  return [
    { url: `${site.url}/`, changeFrequency: "weekly", priority: 1 },
    { url: `${site.url}/levante`, changeFrequency: "weekly", priority: 0.9 },
    { url: `${site.url}/artigos`, changeFrequency: "daily", priority: 0.8 },
    { url: `${site.url}/sobre`, changeFrequency: "monthly", priority: 0.5 },
    { url: `${site.url}/politica-privacidade`, changeFrequency: "yearly", priority: 0.3 },
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
