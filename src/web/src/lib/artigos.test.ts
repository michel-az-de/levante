import { describe, expect, it } from "vitest";
import { extrairTitulos, slugificar, textoDeHeading } from "@/lib/artigos";

describe("slugificar", () => {
  it("remove acento/cedilha e gera kebab-case", () => {
    expect(slugificar("Introdução à Arquitetura")).toBe("introducao-a-arquitetura");
  });
});

describe("textoDeHeading", () => {
  it("reduz link ao rótulo (descarta a URL)", () => {
    expect(textoDeHeading("Veja [o guia](https://x.com)")).toBe("Veja o guia");
  });

  it("remove marcadores de ênfase e código", () => {
    expect(textoDeHeading("Use **git** e `rebase`")).toBe("Use git e rebase");
  });
});

describe("extrairTitulos", () => {
  it("extrai apenas H2 com id e texto limpos", () => {
    const md = "# Titulo\n\n## Primeira\n\ntexto\n\n### Sub\n\n## Segunda";

    expect(extrairTitulos(md)).toEqual([
      { id: "primeira", texto: "Primeira" },
      { id: "segunda", texto: "Segunda" },
    ]);
  });

  it("heading com link: id sem a URL e texto sem a sintaxe do markdown", () => {
    const [titulo] = extrairTitulos("## Veja [o guia](https://x.com)");

    expect(titulo).toEqual({ id: "veja-o-guia", texto: "Veja o guia" });
  });
});
