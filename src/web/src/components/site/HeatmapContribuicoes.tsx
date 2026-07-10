import { Idioma } from "@/components/Idioma";
import type { CalendarioContribuicoes, NivelContribuicao } from "@/types/github";

const CLASSE_NIVEL: Record<NivelContribuicao, string> = {
  0: "bg-site-fg/10",
  1: "bg-site-acc/30",
  2: "bg-site-acc/50",
  3: "bg-site-acc/75",
  4: "bg-site-acc",
};

const SEMANAS_VAZIAS = 53;
const DIAS_SEMANA = 7;

type Celula = { chave: string; nivel: NivelContribuicao; titulo: string | null };

function celulas(calendario: CalendarioContribuicoes | null): Celula[] {
  if (!calendario) {
    // Sem token/erro: grade neutra e honesta (nunca dado inventado — ADR 0006).
    return Array.from({ length: SEMANAS_VAZIAS * DIAS_SEMANA }, (_, i) => ({
      chave: `vazio-${i}`,
      nivel: 0 as NivelContribuicao,
      titulo: null,
    }));
  }
  return calendario.semanas.flatMap((semana, s) =>
    semana.map((dia, d) => ({
      chave: `${s}-${d}`,
      nivel: dia.nivel,
      titulo: `${dia.total} em ${dia.data}`,
    })),
  );
}

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
        {celulas(calendario).map((celula) => (
          <i
            key={celula.chave}
            title={celula.titulo ?? undefined}
            className={`aspect-square w-full ${CLASSE_NIVEL[celula.nivel]}`}
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
