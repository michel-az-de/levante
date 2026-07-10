import { describe, expect, it } from "vitest";
import { filtrarComandos } from "@/lib/cmdk";

const itens = [
  { pt: "Consultoria", en: "Consulting" },
  { pt: "Artigos", en: "Articles" },
  { pt: "Contato", en: "Contact" },
];

describe("filtrarComandos", () => {
  it("sem busca retorna todos", () => {
    expect(filtrarComandos(itens, "")).toHaveLength(3);
  });

  it("filtra por substring em pt", () => {
    expect(filtrarComandos(itens, "consu").map((i) => i.pt)).toEqual(["Consultoria"]);
  });

  it("filtra por substring em en, ignorando caixa", () => {
    expect(filtrarComandos(itens, "ARTIC").map((i) => i.en)).toEqual(["Articles"]);
  });

  it("sem correspondencia retorna vazio", () => {
    expect(filtrarComandos(itens, "xyz")).toEqual([]);
  });
});
