import { JetBrains_Mono, Space_Grotesk } from "next/font/google";
import type { ReactNode } from "react";
import { IdiomaProvider } from "@/lib/i18n/IdiomaProvider";
import { FooterProduto } from "./levante/_components/FooterProduto";
import { NavProduto } from "./levante/_components/NavProduto";

const spaceGrotesk = Space_Grotesk({
  subsets: ["latin"],
  variable: "--font-space-grotesk",
  display: "swap",
});
const jetbrainsMono = JetBrains_Mono({
  subsets: ["latin"],
  variable: "--font-jetbrains-mono",
  display: "swap",
});

/**
 * Casca da landing do produto (marca brass/jade, dark-only, fontes proprias).
 * Idioma reaproveita o mesmo IdiomaProvider; nao ha toggle de tema aqui.
 */
export default function ProdutoLayout({ children }: { children: ReactNode }) {
  return (
    <IdiomaProvider>
      <div
        data-surface="produto"
        className={`${spaceGrotesk.variable} ${jetbrainsMono.variable} min-h-dvh bg-produto-bg font-produto-sans text-produto-fg`}
      >
        <NavProduto />
        <main>{children}</main>
        <FooterProduto />
      </div>
    </IdiomaProvider>
  );
}
