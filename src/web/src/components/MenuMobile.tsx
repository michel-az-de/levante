"use client";

import Link from "next/link";
import { useState } from "react";
import { Idioma } from "@/components/Idioma";
import { useIdioma } from "@/lib/i18n/IdiomaProvider";

export type ItemMenuMobile = { href: string; pt: string; en: string; externo?: boolean };

/**
 * Menu de navegacao para viewports estreitos: nos breakpoints em que o nav
 * horizontal dos headers some, este toggle expoe os mesmos itens num painel
 * colapsavel logo abaixo da barra (o painel ancora no nav sticky do chamador).
 * Clicar num item fecha o painel. Os tokens visuais (site-* ou produto-*)
 * chegam por props para o componente servir as duas superficies.
 */
export function MenuMobile({
  itens,
  id,
  classeContainer,
  classeBotao,
  classePainel,
  classeItem,
}: {
  itens: readonly ItemMenuMobile[];
  id: string;
  classeContainer: string;
  classeBotao: string;
  classePainel: string;
  classeItem: string;
}) {
  const { t } = useIdioma();
  const [aberto, setAberto] = useState(false);

  function fechar() {
    setAberto(false);
  }

  return (
    <div className={classeContainer}>
      <button
        type="button"
        aria-expanded={aberto}
        aria-controls={id}
        aria-label={t(aberto ? "menuFechar" : "menuAbrir")}
        onClick={() => setAberto((atual) => !atual)}
        className={classeBotao}
      >
        <span aria-hidden="true">{aberto ? "✕" : "☰"}</span>
      </button>

      {aberto ? (
        <div id={id} className={classePainel}>
          {itens.map((item) =>
            item.externo ? (
              <a
                key={item.href}
                href={item.href}
                target="_blank"
                rel="noopener noreferrer"
                className={classeItem}
                onClick={fechar}
              >
                <Idioma pt={item.pt} en={item.en} />
              </a>
            ) : item.href.startsWith("#") ? (
              <a key={item.href} href={item.href} className={classeItem} onClick={fechar}>
                <Idioma pt={item.pt} en={item.en} />
              </a>
            ) : (
              <Link key={item.href} href={item.href} className={classeItem} onClick={fechar}>
                <Idioma pt={item.pt} en={item.en} />
              </Link>
            ),
          )}
        </div>
      ) : null}
    </div>
  );
}
