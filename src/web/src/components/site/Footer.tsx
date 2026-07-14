import Link from "next/link";
import { Idioma } from "@/components/Idioma";

/** Rodape do site pessoal. */
export function Footer() {
  return (
    <footer className="border-t border-site-line py-[30px]">
      <div className="mx-auto flex max-w-[1180px] flex-wrap items-center justify-between gap-[18px] px-[clamp(18px,4vw,40px)] text-[13px] text-site-faint">
        <Link href="/" className="text-[15px] text-site-fg transition-colors hover:text-site-acc">
          Felipe Azevedo
        </Link>
        <div className="flex items-center gap-[18px]">
          <Link href="/sobre" className="transition-colors hover:text-site-acc">
            <Idioma pt="Sobre" en="About" />
          </Link>
          <Link href="/politica-privacidade" className="transition-colors hover:text-site-acc">
            <Idioma pt="Privacidade" en="Privacy" />
          </Link>
          <span>
            <Idioma pt="Feito com Levante" en="Built with Levante" />
          </span>
          <span>© 2026</span>
        </div>
      </div>
    </footer>
  );
}
