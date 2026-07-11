import { describe, expect, it } from "vitest";
import { montarCelulas } from "@/lib/github/heatmap";
import type { CalendarioContribuicoes } from "@/types/github";

describe("montarCelulas (heatmap)", () => {
  it("sem calendário: grade neutra 53x7, sem anteparo", () => {
    const celulas = montarCelulas(null);

    expect(celulas).toHaveLength(53 * 7);
    expect(celulas.some((c) => c.nivel === null)).toBe(false);
  });

  it("1ª semana parcial: prefixa anteparo até o offset do 1º dia (quarta = 3)", () => {
    // 2026-01-07 é uma quarta-feira (getUTCDay = 3) → 3 células de anteparo.
    const calendario: CalendarioContribuicoes = {
      total: 4,
      semanas: [
        [
          { data: "2026-01-07", total: 1, nivel: 1 },
          { data: "2026-01-08", total: 0, nivel: 0 },
          { data: "2026-01-09", total: 3, nivel: 2 },
        ],
        [
          { data: "2026-01-11", total: 0, nivel: 0 },
          { data: "2026-01-12", total: 0, nivel: 0 },
          { data: "2026-01-13", total: 0, nivel: 0 },
          { data: "2026-01-14", total: 0, nivel: 0 },
          { data: "2026-01-15", total: 0, nivel: 0 },
          { data: "2026-01-16", total: 0, nivel: 0 },
          { data: "2026-01-17", total: 0, nivel: 0 },
        ],
      ],
    };

    const celulas = montarCelulas(calendario);

    // Anteparo (nivel null) só na liderança, sem título; depois os dias reais.
    expect(celulas.slice(0, 3).every((c) => c.nivel === null && c.titulo === null)).toBe(true);
    expect(celulas.filter((c) => c.nivel === null)).toHaveLength(3);
    expect(celulas[3].titulo).toBe("1 em 2026-01-07");
    // Total = anteparo (3) + dias (3 + 7).
    expect(celulas).toHaveLength(3 + 3 + 7);
  });

  it("1ª semana começa no domingo: sem anteparo", () => {
    // 2026-01-04 é um domingo (getUTCDay = 0).
    const calendario: CalendarioContribuicoes = {
      total: 0,
      semanas: [[{ data: "2026-01-04", total: 0, nivel: 0 }]],
    };

    expect(montarCelulas(calendario).some((c) => c.nivel === null)).toBe(false);
  });
});
