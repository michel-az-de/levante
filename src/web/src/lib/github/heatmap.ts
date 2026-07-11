// Montagem das celulas do heatmap de contribuicoes (fatia E3, ADR 0006). Logica pura
// (sem React/Tailwind) para ser testavel: o componente so mapeia o nivel para a cor.

import type { CalendarioContribuicoes, NivelContribuicao } from "@/types/github";

const SEMANAS_VAZIAS = 53;
const DIAS_SEMANA = 7;

export type CelulaHeatmap = {
  chave: string;
  /** Intensidade do dia, ou <c>null</c> para anteparo (dias antes do inicio da janela). */
  nivel: NivelContribuicao | null;
  titulo: string | null;
};

/**
 * Offset do 1o dia registrado (0=domingo..6=sabado). O calendario do GitHub comeca no
 * domingo e a 1a semana costuma ser parcial; esse offset diz quantas celulas de anteparo
 * inserir antes do 1o dia para alinhar as linhas de dia-da-semana.
 */
function offsetDoPrimeiroDia(calendario: CalendarioContribuicoes): number {
  const primeiro = calendario.semanas[0]?.[0]?.data;
  if (!primeiro) {
    return 0;
  }
  const diaDaSemana = new Date(`${primeiro}T00:00:00Z`).getUTCDay();
  return Number.isNaN(diaDaSemana) ? 0 : diaDaSemana;
}

/**
 * Monta as celulas na ordem de renderizacao (colunas = semanas, 7 linhas). Sem o anteparo
 * de lideranca, o `grid-flow-col`/`grid-rows-7` preenche as colunas de cima para baixo e a
 * 1a semana parcial empurra todos os dias para cima, cisalhando a grade. As celulas de
 * anteparo (nivel null) reservam a posicao sem fingir dias sem contribuicao.
 */
export function montarCelulas(calendario: CalendarioContribuicoes | null): CelulaHeatmap[] {
  if (!calendario) {
    // Sem token/erro: grade neutra e honesta (nunca dado inventado — ADR 0006).
    return Array.from({ length: SEMANAS_VAZIAS * DIAS_SEMANA }, (_, i) => ({
      chave: `vazio-${i}`,
      nivel: 0 as NivelContribuicao,
      titulo: null,
    }));
  }

  const anteparo: CelulaHeatmap[] = Array.from(
    { length: offsetDoPrimeiroDia(calendario) },
    (_, i) => ({ chave: `anteparo-${i}`, nivel: null, titulo: null }),
  );

  const dias = calendario.semanas.flatMap((semana, s) =>
    semana.map((dia, d) => ({
      chave: `${s}-${d}`,
      nivel: dia.nivel,
      titulo: `${dia.total} em ${dia.data}`,
    })),
  );

  return [...anteparo, ...dias];
}
