import { cleanup, render } from "@testing-library/react";
import { afterEach, beforeAll, describe, expect, it, vi } from "vitest";
import { artigoApi } from "@/lib/api";
import HomePage from "./page";

vi.mock("@/lib/api", () => ({ artigoApi: { GET: vi.fn() } }));
// Secoes sem relacao com a numeracao (e que buscam dados externos) ficam fora do teste.
vi.mock("@/components/JsonLd", () => ({ JsonLd: () => null }));
vi.mock("@/components/site/HeroTerminal", () => ({ HeroTerminal: () => null }));
vi.mock("@/components/site/SecaoConsultoria", () => ({ SecaoConsultoria: () => null }));
vi.mock("@/components/site/SecaoCapacidades", () => ({ SecaoCapacidades: () => null }));
vi.mock("@/components/site/SecaoCodigoAberto", () => ({ SecaoCodigoAberto: () => null }));

const artigoGet = vi.mocked(artigoApi.GET);

const artigo = {
  slug: "primeiro",
  titulo: "Primeiro artigo",
  resumo: "Resumo",
  conteudo: "texto",
  tags: ["dotnet"],
  dataPublicacao: "2026-11-24T00:00:00Z",
};

async function renderizarHome(artigos: unknown[]) {
  artigoGet.mockResolvedValue({ data: artigos } as never);
  return render(await HomePage());
}

function numeroDa(container: HTMLElement, id: string) {
  return container.querySelector(`#${id} b`)?.textContent;
}

describe("HomePage", () => {
  // O botao magnetico do contato consulta matchMedia, que o jsdom nao implementa.
  beforeAll(() => {
    window.matchMedia = ((query: string) => ({ matches: false, media: query })) as never;
  });
  afterEach(cleanup);

  it("sem artigo publicado esconde a secao 04 e sobe experiencia e contato", async () => {
    const { container } = await renderizarHome([]);

    expect(container.querySelector("#artigos")).toBeNull();
    expect(numeroDa(container, "experiencia")).toBe("04");
    expect(numeroDa(container, "contato")).toBe("05");
  });

  it("com artigo publicado mostra a secao 04 e mantem 05 e 06", async () => {
    const { container } = await renderizarHome([artigo]);

    expect(numeroDa(container, "artigos")).toBe("04");
    expect(numeroDa(container, "experiencia")).toBe("05");
    expect(numeroDa(container, "contato")).toBe("06");
  });
});
