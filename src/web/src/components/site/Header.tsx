"use client";

import Link from "next/link";
import { Idioma } from "@/components/Idioma";
import { IdiomaToggle } from "@/components/IdiomaToggle";
import { TemaToggle } from "@/components/TemaToggle";
import { useScrollSpy } from "@/lib/hooks/useScrollSpy";
import { Botao } from "./Botao";
import { EVENTO_ABRIR_CMDK } from "./CmdK";
import { Marca } from "./Marca";

const SECOES = ["consultoria", "capacidades", "github", "experiencia"] as const;

type ItemNav = { pt: string; en: string } & ({ secao: string } | { rota: string });

const ITENS: readonly ItemNav[] = [
  { secao: "consultoria", pt: "Consultoria", en: "Consulting" },
  { secao: "capacidades", pt: "O que faço", en: "What I do" },
  { secao: "github", pt: "Código", en: "Code" },
  { rota: "/artigos", pt: "Artigos", en: "Writing" },
  { secao: "experiencia", pt: "Experiência", en: "Experience" },
];

const BASE_LINK =
  "relative py-0.5 text-sm text-site-fg2 transition-colors hover:text-site-fg after:absolute after:-bottom-0.5 after:left-0 after:h-px after:w-0 after:bg-site-acc after:transition-[width] hover:after:w-full";
const ICOBTN =
  "border border-site-line2 px-2.5 py-2 font-site-mono text-xs text-site-fg2 transition-colors hover:border-site-acc hover:text-site-acc";

/** Nav fixo do site: marca, secoes (scrollspy), toggles, cmd-k e CTA. */
export function Header() {
  const ativa = useScrollSpy(SECOES);

  function abrirCmdK() {
    window.dispatchEvent(new Event(EVENTO_ABRIR_CMDK));
  }

  return (
    <nav className="sticky top-0 z-40 border-b border-site-line bg-site-bg/85 backdrop-blur-[12px]">
      <div className="mx-auto flex max-w-[1180px] items-center gap-6 px-[clamp(18px,4vw,40px)] py-[15px]">
        <Link
          href="/"
          className="flex items-center gap-[11px] text-base font-bold tracking-tight text-site-fg"
        >
          <Marca className="h-[19px] w-[17px]" /> Felipe Azevedo
        </Link>

        <div className="ml-1.5 hidden gap-6 min-[860px]:flex">
          {ITENS.map((item) =>
            "rota" in item ? (
              <Link key={item.rota} href={item.rota} className={BASE_LINK}>
                <Idioma pt={item.pt} en={item.en} />
              </Link>
            ) : (
              <a
                key={item.secao}
                href={`#${item.secao}`}
                className={`${BASE_LINK}${ativa === item.secao ? " text-site-fg after:w-full" : ""}`}
              >
                <Idioma pt={item.pt} en={item.en} />
              </a>
            ),
          )}
        </div>

        <div className="ml-auto flex items-center gap-2">
          <button
            type="button"
            onClick={abrirCmdK}
            aria-label="Abrir paleta de comandos"
            className={ICOBTN}
          >
            ⌘K
          </button>
          <IdiomaToggle className={ICOBTN} />
          <TemaToggle className={ICOBTN} />
          <span className="hidden sm:inline-flex">
            <Botao href="#contato" variante="acc" tamanho="sm" magnetico>
              <Idioma pt="Falar comigo" en="Get in touch" />
            </Botao>
          </span>
        </div>
      </div>
    </nav>
  );
}
