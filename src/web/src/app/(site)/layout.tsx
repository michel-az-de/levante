import { IBM_Plex_Mono, Schibsted_Grotesk } from "next/font/google";
import type { ReactNode } from "react";
import { CmdK } from "@/components/site/CmdK";
import { Footer } from "@/components/site/Footer";
import { Header } from "@/components/site/Header";
import { Spotlight } from "@/components/site/Spotlight";
import { IdiomaProvider } from "@/lib/i18n/IdiomaProvider";

const schibsted = Schibsted_Grotesk({
  subsets: ["latin"],
  variable: "--font-schibsted",
  display: "swap",
});
const plexMono = IBM_Plex_Mono({
  subsets: ["latin"],
  weight: ["400", "500"],
  variable: "--font-plex-mono",
  display: "swap",
});

/**
 * Casca do site pessoal (marca teal, tema claro/escuro, chrome bilingue). As
 * fontes ficam aqui (nao no root) para nao pre-carregar em outras superficies.
 */
export default function SiteLayout({ children }: { children: ReactNode }) {
  return (
    <IdiomaProvider>
      <div
        data-surface="site"
        className={`${schibsted.variable} ${plexMono.variable} relative min-h-dvh bg-site-bg font-site-sans text-site-fg`}
      >
        <div className="site-grain" aria-hidden="true" />
        <div className="site-ambient" aria-hidden="true" />
        <Spotlight />
        <Header />
        <main className="relative z-[1]">{children}</main>
        <Footer />
        <CmdK />
      </div>
    </IdiomaProvider>
  );
}
