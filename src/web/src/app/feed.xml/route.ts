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

// Lanca em falha (sem catch aqui de proposito): o unstable_cache nao memoriza
// rejeicao, entao um blip da API nao congela um feed vazio por 1h.
async function listarArtigos(): Promise<Artigo[]> {
  const { data, response } = await artigoApi.GET("/artigos");
  if (!response.ok) {
    throw new Error(`Feed: /artigos respondeu HTTP ${response.status}`);
  }
  return data ?? [];
}

// Dados cacheados (revalidate 1h): a rota fica force-dynamic para ler SITE_URL em runtime,
// mas nao refaz a chamada de API a cada request. Ver issue #84.
const artigosCacheados = unstable_cache(listarArtigos, ["feed:artigos"], { revalidate: 3600 });

export async function GET(): Promise<Response> {
  let artigos: Artigo[];
  let falhou = false;
  try {
    artigos = await artigosCacheados();
  } catch {
    // Degrada para o feed so com o canal; a falha nao entra em nenhum cache.
    artigos = [];
    falhou = true;
  }

  const itens = artigos
    .map((artigo) => {
      const link = `${site.url}/artigos/${artigo.slug}`;
      const pubDate = artigo.dataPublicacao
        ? `<pubDate>${new Date(artigo.dataPublicacao).toUTCString()}</pubDate>`
        : "";
      return `    <item>
      <title>${escaparXml(artigo.titulo)}</title>
      <link>${escaparXml(link)}</link>
      <guid>${escaparXml(link)}</guid>
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
      // Feed degradado por falha nao pode ficar preso em cache downstream.
      "Cache-Control": falhou
        ? "no-store"
        : "public, s-maxage=3600, stale-while-revalidate=86400",
    },
  });
}
