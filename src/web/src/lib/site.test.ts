import { describe, expect, it } from "vitest";
import { site } from "@/lib/site";

describe("site", () => {
  it("usa o nome do autor como nome do site (titulo da aba, OG e feed)", () => {
    expect(site.nome).toBe("Felipe Michel de Azevedo");
  });

  it("aponta o LinkedIn para o perfil ativo e o publica no sameAs", () => {
    expect(site.linkedin).toBe("https://www.linkedin.com/in/felipemichelazevedo");
    expect(site.sameAs).toContain(site.linkedin);
  });
});
