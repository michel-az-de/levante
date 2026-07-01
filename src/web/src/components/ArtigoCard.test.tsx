import { cleanup, render, screen } from "@testing-library/react";
import { afterEach, describe, expect, it } from "vitest";
import { ArtigoCard } from "@/components/ArtigoCard";
import type { Artigo } from "@/types/domain";

const artigoBase: Artigo = {
  id: "a1",
  titulo: "Clean Architecture no Levante",
  slug: "clean-architecture-no-levante",
  resumo: "Como o monolito modular foi fatiado.",
  conteudo: "...",
  status: "Publicado",
  dataPublicacao: "2026-01-15T12:00:00Z",
  metaTitulo: null,
  metaDescricao: null,
  imagemOgUrl: null,
  categoriaId: null,
  tags: ["dotnet", "ddd"],
};

describe("ArtigoCard", () => {
  afterEach(cleanup);

  it("renderiza titulo como link para o slug, resumo e tags", () => {
    render(<ArtigoCard artigo={artigoBase} />);

    const link = screen.getByRole("link", { name: artigoBase.titulo });
    expect(link.getAttribute("href")).toBe("/artigos/clean-architecture-no-levante");
    expect(screen.getByText(artigoBase.resumo)).toBeTruthy();
    expect(screen.getByText("#dotnet")).toBeTruthy();
    expect(screen.getByText("#ddd")).toBeTruthy();
  });

  it("mostra a categoria quando informada e omite quando ausente", () => {
    const { rerender } = render(<ArtigoCard artigo={artigoBase} categoriaNome="Arquitetura" />);
    expect(screen.getByText("Arquitetura")).toBeTruthy();

    rerender(<ArtigoCard artigo={artigoBase} />);
    expect(screen.queryByText("Arquitetura")).toBeNull();
  });

  it("omite a data quando o artigo nao tem dataPublicacao", () => {
    render(<ArtigoCard artigo={{ ...artigoBase, dataPublicacao: null }} />);
    expect(screen.queryByText(/Publicado em/)).toBeNull();
  });
});
