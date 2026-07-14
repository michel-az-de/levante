import type { MetadataRoute } from "next";
import { siteIndexavel } from "@/lib/flags";
import { site } from "@/lib/site";

// force-dynamic: le SITE_URL/flag em runtime (o cutover D0 e restart, nao rebuild;
// senao o prerender de build assaria a URL/flag do build).
export const dynamic = "force-dynamic";

export default function robots(): MetadataRoute.Robots {
  if (!siteIndexavel()) {
    // Host provisorio / pre-cutover: nao expor nada aos buscadores (nem o sitemap).
    return { rules: { userAgent: "*", disallow: "/" } };
  }
  return {
    rules: { userAgent: "*", allow: "/", disallow: "/admin" },
    sitemap: `${site.url}/sitemap.xml`,
    host: site.url,
  };
}
