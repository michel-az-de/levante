import { cleanup, render, screen } from "@testing-library/react";
import { afterEach, describe, expect, it } from "vitest";
import { SecaoArtigos } from "@/components/site/SecaoArtigos";
import type { Artigo } from "@/types/domain";

describe("SecaoArtigos", () => {
  afterEach(cleanup);

  it("nao renderiza nada quando nao ha artigo publicado", () => {
    expect(SecaoArtigos({ artigos: [] })).toBeNull();
  });

  it("mostra o artigo em destaque quando ha publicacao", () => {
    const artigo = {
      slug: "primeiro",
      titulo: "Primeiro artigo",
      resumo: "Resumo",
      conteudo: "texto",
      tags: ["dotnet"],
      dataPublicacao: "2026-11-24T00:00:00Z",
    } as unknown as Artigo;

    render(<>{SecaoArtigos({ artigos: [artigo] })}</>);

    expect(screen.getByText("Primeiro artigo")).toBeTruthy();
  });
});
