import { Idioma } from "@/components/Idioma";
import type { CardCapacidade as DadosCapacidade } from "@/lib/site-conteudo";

/** Card de uma capacidade entregue (secao O que eu entrego). */
export function CardCapacidade({ capacidade }: { capacidade: DadosCapacidade }) {
  return (
    <div className="relative bg-site-bg p-7 transition-colors hover:bg-site-bg1">
      <span className="absolute right-6 top-6 font-site-mono text-xs text-site-faint">
        {capacidade.numero}
      </span>
      <div
        aria-hidden="true"
        className="mb-4 flex h-10 w-10 items-center justify-center border border-site-acc/35 bg-site-acc/12 text-[17px] text-site-acc"
      >
        {capacidade.icone}
      </div>
      <h3 className="mb-2.5 text-lg font-semibold tracking-tight text-site-fg">
        <Idioma pt={capacidade.titulo.pt} en={capacidade.titulo.en} />
      </h3>
      <p className="text-sm leading-relaxed text-site-fg2">
        <Idioma pt={capacidade.descricao.pt} en={capacidade.descricao.en} />
      </p>
    </div>
  );
}
