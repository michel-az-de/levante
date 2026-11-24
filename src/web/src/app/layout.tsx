import type { Metadata } from "next";
import { site } from "@/lib/site";
import { scriptBootTemaIdioma } from "@/lib/tema-idioma-boot";
import "./globals.css";

// SITE_URL e de runtime (ADR 0007: a imagem nao conhece o dominio). Pagina pre-renderizada no
// `next build` do CI assaria o fallback localhost no canonical, no OG e no metadataBase; por isso
// todas as rotas renderizam no request e leem o SITE_URL do container (#177).
export const dynamic = "force-dynamic";

export const metadata: Metadata = {
  metadataBase: new URL(site.url),
  title: { default: site.nome, template: `%s — ${site.nome}` },
  description: site.descricao,
  openGraph: {
    siteName: site.nome,
    locale: "pt_BR",
    type: "website",
  },
  twitter: { card: "summary_large_image" },
};

export default function RootLayout({
  children,
}: Readonly<{ children: React.ReactNode }>) {
  return (
    <html lang="pt-BR" data-theme="dark" data-idioma="pt" suppressHydrationWarning>
      <body>
        {/* Anti-FOUC: restaura tema/idioma salvos antes do primeiro paint. */}
        <script dangerouslySetInnerHTML={{ __html: scriptBootTemaIdioma }} />
        {children}
      </body>
    </html>
  );
}
