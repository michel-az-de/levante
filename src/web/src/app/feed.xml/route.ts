import { unstable_cache } from "next/cache";
import { artigoApi } from "@/lib/api";
import { site } from "@/lib/site";
import type { Artigo } from "@/types/domain";

// force-dynamic: le SITE_URL em runtime (cutover = restart, nao rebuild).
export const dynamic = "force-dynamic";

function escaparXml(texto: string): string {
  return texto
    .replaceAll("&", "&amp;")
    .replaceAll("<", "&lt;")
    .replaceAll(">", "&gt;")
    .replaceAll('"', "&quot;")
    .replaceAll("'", "&apos;");
}

async function listarArtigos(): Promise<Artigo[]> {
  try {
    const { data } = await artigoApi.GET("/artigos");
    return data ?? [];
  } catch {
    return [];
  }
}

// Dados cacheados (revalidate 1h): a rota fica force-dynamic para ler SITE_URL em runtime,
// mas nao refaz a chamada de API a cada request. Ver issue #84.
const artigosCacheados = unstable_cache(listarArtigos, ["feed:artigos"], { revalidate: 3600 });

export async function GET(): Promise<Response> {
  const artigos = await artigosCacheados();

  const itens = artigos
    .map((artigo) => {
      const link = `${site.url}/artigos/${artigo.slug}`;
      const pubDate = artigo.dataPublicacao
        ? `<pubDate>${new Date(artigo.dataPublicacao).toUTCString()}</pubDate>`
        : "";
      return `    <item>
      <title>${escaparXml(artigo.titulo)}</title>
      <link>${link}</link>
      <guid>${link}</guid>
      <description>${escaparXml(artigo.resumo)}</description>
      ${pubDate}
    </item>`;
    })
    .join("\n");

  const xml = `<?xml version="1.0" encoding="UTF-8"?>
<rss version="2.0">
  <channel>
    <title>${escaparXml(site.nome)}</title>
    <link>${site.url}</link>
    <description>${escaparXml(site.descricao)}</description>
    <language>pt-BR</language>
${itens}
  </channel>
</rss>`;

  return new Response(xml, {
    headers: {
      "Content-Type": "application/rss+xml; charset=utf-8",
      // Cache HTTP (issue #84): permite um cache downstream absorver hits repetidos de crawler
      // sem reintroduzir o prerender de build (que assaria SITE_URL do build).
      "Cache-Control": "public, s-maxage=3600, stale-while-revalidate=86400",
    },
  });
}
