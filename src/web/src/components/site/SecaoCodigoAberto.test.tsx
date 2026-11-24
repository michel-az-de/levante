import { cleanup, render } from "@testing-library/react";
import { afterEach, beforeAll, describe, expect, it, vi } from "vitest";
import { SecaoCodigoAberto } from "@/components/site/SecaoCodigoAberto";

// Sem token a busca ainda chama a rede; aqui a secao cai nos valores curados.
vi.mock("@/lib/github/repositorio", () => ({
  buscarRepositorioGithub: vi.fn().mockResolvedValue(null),
}));
vi.mock("@/lib/github/contribuicoes", () => ({
  buscarContribuicoesGithub: vi.fn().mockResolvedValue(null),
}));

describe("SecaoCodigoAberto", () => {
  // O RevealOnScroll consulta matchMedia, que o jsdom nao implementa.
  beforeAll(() => {
    window.matchMedia = ((query: string) => ({ matches: true, media: query })) as never;
  });
  afterEach(cleanup);

  it("nao linka os repositorios que respondiam 404", async () => {
    const { container } = render(await SecaoCodigoAberto());

    const hrefs = Array.from(container.querySelectorAll("a")).map((a) => a.getAttribute("href"));
    const fechados = hrefs.filter((h) => /\/(oracle-pack|easystock|hiram)$/.test(h ?? ""));
    expect(fechados).toEqual([]);
    expect(hrefs.filter((h) => h === "https://github.com/michel-az-de/levante")).toHaveLength(1);
  });
});
