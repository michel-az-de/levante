import { describe, expect, it } from "vitest";
import { criarLimitador } from "./rate-limit-memoria";

describe("criarLimitador (rate limit in-memory)", () => {
  it("permite ate o limite e bloqueia o excedente na mesma janela", () => {
    const permitir = criarLimitador(3, 60_000);
    const t = 1_000;
    expect(permitir("ip-a", t)).toBe(true);
    expect(permitir("ip-a", t)).toBe(true);
    expect(permitir("ip-a", t)).toBe(true);
    expect(permitir("ip-a", t)).toBe(false);
  });

  it("reseta a contagem quando a janela expira", () => {
    const permitir = criarLimitador(1, 60_000);
    expect(permitir("ip-a", 1_000)).toBe(true);
    expect(permitir("ip-a", 1_000)).toBe(false);
    // apos a janela (>= reset), volta a permitir
    expect(permitir("ip-a", 61_000)).toBe(true);
  });

  it("isola a contagem por chave", () => {
    const permitir = criarLimitador(1, 60_000);
    expect(permitir("ip-a", 1_000)).toBe(true);
    expect(permitir("ip-b", 1_000)).toBe(true);
    expect(permitir("ip-a", 1_000)).toBe(false);
  });
});
