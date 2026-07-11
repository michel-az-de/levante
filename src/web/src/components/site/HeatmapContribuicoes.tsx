import { Idioma } from "@/components/Idioma";
import { montarCelulas } from "@/lib/github/heatmap";
import type { CalendarioContribuicoes, NivelContribuicao } from "@/types/github";

const CLASSE_NIVEL: Record<NivelContribuicao, string> = {
  0: "bg-site-fg/10",
  1: "bg-site-acc/30",
  2: "bg-site-acc/50",
  3: "bg-site-acc/75",
  4: "bg-site-acc",
};

/** Heatmap de contribuicoes do GitHub (12 meses). Colunas = semanas, 7 linhas. */
export function HeatmapContribuicoes({
  calendario,
}: {
  calendario: CalendarioContribuicoes | null;
}) {
  return (
    <div className="flex h-full flex-col">
      <div className="font-site-mono text-[11px] uppercase tracking-wider text-site-faint">
        <Idioma pt="contribuições · 12 meses" en="contributions · 12 months" />
      </div>
      <div className="mt-auto grid grid-flow-col grid-rows-7 gap-[3px] pt-5">
        {montarCelulas(calendario).map((celula) => (
          <i
            key={celula.chave}
            title={celula.titulo ?? undefined}
            className={`aspect-square w-full ${celula.nivel === null ? "invisible" : CLASSE_NIVEL[celula.nivel]}`}
          />
        ))}
      </div>
      <div className="mt-3.5 flex justify-between font-site-mono text-[11px] text-site-faint">
        <span>
          <Idioma pt="menos" en="less" />
        </span>
        <span>
          <Idioma pt="mais" en="more" />
        </span>
      </div>
    </div>
  );
}
