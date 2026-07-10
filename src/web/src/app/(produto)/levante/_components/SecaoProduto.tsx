import type { ReactNode } from "react";
import { Idioma } from "@/components/Idioma";

/** Casca de uma secao da landing do produto (borda superior + faixa centralizada). */
export function SecaoProduto({ id, children }: { id?: string; children: ReactNode }) {
  return (
    <section id={id} className="border-t border-produto-line py-[clamp(46px,7vw,82px)]">
      <div className="mx-auto max-w-[1140px] px-[clamp(16px,4vw,32px)]">{children}</div>
    </section>
  );
}

type Texto = { pt: ReactNode; en: ReactNode };

/** Cabecalho centralizado de secao (kicker jade + titulo + subtitulo). */
export function CabecalhoProduto({
  kicker,
  titulo,
  subtitulo,
}: {
  kicker: Texto;
  titulo: Texto;
  subtitulo?: Texto;
}) {
  return (
    <div className="mx-auto mb-10 max-w-[60ch] text-center">
      <div className="mb-3 font-produto-mono text-xs uppercase tracking-[0.12em] text-produto-jade">
        <Idioma pt={kicker.pt} en={kicker.en} />
      </div>
      <h2 className="mb-3 text-[clamp(26px,4vw,38px)] font-bold tracking-tight text-produto-fg">
        <Idioma pt={titulo.pt} en={titulo.en} />
      </h2>
      {subtitulo ? (
        <p className="text-base text-produto-dim">
          <Idioma pt={subtitulo.pt} en={subtitulo.en} />
        </p>
      ) : null}
    </div>
  );
}
