import type { ReactNode } from "react";
import { Idioma } from "@/components/Idioma";

/** Casca de uma secao da home (borda superior + faixa centralizada). */
export function Secao({
  id,
  children,
}: {
  id?: string;
  children: ReactNode;
}) {
  return (
    <section id={id} className="border-t border-site-line py-[clamp(60px,9vw,104px)]">
      <div className="mx-auto max-w-[1180px] px-[clamp(18px,4vw,40px)]">{children}</div>
    </section>
  );
}

type Texto = { pt: ReactNode; en: ReactNode };

/** Cabecalho padrao de secao: kicker numerado + titulo + subtitulo + acao. */
export function CabecalhoSecao({
  numero,
  kicker,
  titulo,
  subtitulo,
  acao,
}: {
  numero: string;
  kicker: Texto;
  titulo: Texto;
  subtitulo?: Texto;
  acao?: ReactNode;
}) {
  return (
    <div className="mb-[42px] flex flex-wrap items-end justify-between gap-5">
      <div>
        <div className="site-label mb-4">
          <b className="font-medium text-site-acc">{numero}</b> /{" "}
          <Idioma pt={kicker.pt} en={kicker.en} />
        </div>
        <h2 className="text-[clamp(32px,5.2vw,58px)] leading-[0.98] font-bold tracking-[-0.03em] text-site-fg">
          <Idioma pt={titulo.pt} en={titulo.en} />
        </h2>
        {subtitulo ? (
          <p className="mt-3.5 max-w-[54ch] text-base text-site-fg2">
            <Idioma pt={subtitulo.pt} en={subtitulo.en} />
          </p>
        ) : null}
      </div>
      {acao}
    </div>
  );
}
