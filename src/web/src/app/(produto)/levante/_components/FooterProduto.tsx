import Link from "next/link";
import { Idioma } from "@/components/Idioma";
import { produto } from "@/lib/produto";

/** Rodape da landing do produto. */
export function FooterProduto() {
  return (
    <footer className="border-t border-produto-line py-9">
      <div className="mx-auto flex max-w-[1140px] flex-wrap items-center justify-between gap-4 px-[clamp(16px,4vw,32px)]">
        <div className="flex items-center gap-2.5 text-[15px] font-bold text-produto-fg">
          <span aria-hidden="true" className="h-4 w-4 rotate-45 border-2 border-produto-brass" /> Levante
        </div>
        <div className="flex gap-5 text-[13.5px] text-produto-dim">
          <a
            href={produto.urlRepo}
            target="_blank"
            rel="noopener noreferrer"
            className="transition-colors hover:text-produto-fg"
          >
            GitHub
          </a>
          <a
            href={produto.urlDocs}
            target="_blank"
            rel="noopener noreferrer"
            className="transition-colors hover:text-produto-fg"
          >
            Docs
          </a>
          <Link href="/" className="transition-colors hover:text-produto-fg">
            <Idioma pt="Feito por Felipe" en="Built by Felipe" />
          </Link>
        </div>
        <div className="font-produto-mono text-[11.5px] text-produto-faint">
          da pedra bruta à pedra polida
        </div>
      </div>
    </footer>
  );
}
