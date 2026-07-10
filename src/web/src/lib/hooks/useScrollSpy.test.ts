import { describe, expect, it } from "vitest";
import { escolherSecaoAtiva } from "@/lib/hooks/useScrollSpy";

describe("escolherSecaoAtiva", () => {
  it("retorna a ultima secao cujo topo ja passou do limite", () => {
    const secoes = [
      { id: "a", top: -100 },
      { id: "b", top: 50 },
      { id: "c", top: 500 },
    ];
    expect(escolherSecaoAtiva(secoes, 200)).toBe("b");
  });

  it("retorna null quando nenhuma passou do limite", () => {
    expect(escolherSecaoAtiva([{ id: "a", top: 900 }], 200)).toBeNull();
  });

  it("retorna a ultima quando todas ja passaram", () => {
    expect(
      escolherSecaoAtiva(
        [
          { id: "a", top: 0 },
          { id: "b", top: 10 },
        ],
        200,
      ),
    ).toBe("b");
  });
});
