"use client";

import Link from "next/link";
import { Idioma } from "@/components/Idioma";
import { IdiomaToggle } from "@/components/IdiomaToggle";
import { produto } from "@/lib/produto";

type ItemLink = { href: string; pt: string; en: string; externo?: boolean };

const LINKS: readonly ItemLink[] = [
  { href: "#recursos", pt: "Recursos", en: "Features" },
  { href: "#arquitetura", pt: "Arquitetura", en: "Architecture" },
  { href: "#repo", pt: "Código", en: "Code" },
  { href: "#autor", pt: "Autor", en: "Author" },
  { href: produto.urlDocs, pt: "Docs", en: "Docs", externo: true },
];

const ITEM = "text-[13.5px] font-medium text-produto-dim transition-colors hover:text-produto-fg";

/** Nav fixo da landing do produto (dark-only: sem toggle de tema). */
export function NavProduto() {
  return (
    <nav className="sticky top-0 z-40 border-b border-produto-line bg-produto-bg/90 backdrop-blur-[10px]">
      <div className="mx-auto flex max-w-[1140px] items-center gap-[22px] px-[clamp(16px,4vw,32px)] py-3.5">
        <Link
          href="/levante"
          className="flex items-center gap-[11px] text-[17px] font-bold tracking-[0.04em] text-produto-fg"
        >
          <span aria-hidden="true" className="h-4 w-4 rotate-45 border-2 border-produto-brass" />{" "}
          Levante
        </Link>

        <div className="ml-2 hidden gap-5 min-[720px]:flex">
          {LINKS.map((item) =>
            item.externo ? (
              <a
                key={item.href}
                href={item.href}
                target="_blank"
                rel="noopener noreferrer"
                className={ITEM}
              >
                <Idioma pt={item.pt} en={item.en} />
              </a>
            ) : (
              <a key={item.href} href={item.href} className={ITEM}>
                <Idioma pt={item.pt} en={item.en} />
              </a>
            ),
          )}
        </div>

        <div className="ml-auto flex items-center gap-2.5">
          <IdiomaToggle className="rounded-lg border border-produto-line2 px-2.5 py-1.5 font-produto-mono text-xs text-produto-dim transition-colors hover:border-produto-brass hover:text-produto-fg" />
          <a
            href={produto.urlRepo}
            target="_blank"
            rel="noopener noreferrer"
            className="hidden rounded-lg border border-produto-line2 px-3 py-1.5 text-[13.5px] font-semibold text-produto-fg transition-colors hover:border-produto-brass sm:inline-flex"
          >
            <span aria-hidden="true" className="mr-1 text-xs opacity-80">
              ★
            </span>{" "}
            GitHub
          </a>
          <Link
            href="/admin/login"
            className="rounded-lg border border-produto-jade bg-produto-jade px-3 py-1.5 text-[13.5px] font-semibold text-produto-jadetx transition hover:brightness-110"
          >
            <Idioma pt="Começar" en="Get started" />
          </Link>
        </div>
      </div>
    </nav>
  );
}
