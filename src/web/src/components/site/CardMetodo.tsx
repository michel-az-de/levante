import { Idioma } from "@/components/Idioma";
import type { CardMetodo as DadosMetodo } from "@/lib/site-conteudo";

/** Card de um passo do metodo de entrega (secao Consultoria). */
export function CardMetodo({ metodo }: { metodo: DadosMetodo }) {
  return (
    <div className="relative bg-site-bg p-7 transition-colors hover:bg-site-bg1">
      <span className="mb-4 block font-site-mono text-xs text-site-acc">{metodo.numero}</span>
      <h3 className="mb-2.5 text-lg font-semibold tracking-tight text-site-fg">
        <Idioma pt={metodo.titulo.pt} en={metodo.titulo.en} />
      </h3>
      <p className="text-sm leading-relaxed text-site-fg2">
        <Idioma pt={metodo.descricao.pt} en={metodo.descricao.en} />
      </p>
    </div>
  );
}
