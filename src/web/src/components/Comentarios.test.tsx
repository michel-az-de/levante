import { cleanup, render, screen, waitFor } from "@testing-library/react";
import { afterEach, describe, expect, it, vi } from "vitest";
import { Comentarios } from "@/components/Comentarios";

function respostaJson(dados: unknown): Response {
  return new Response(JSON.stringify(dados), {
    status: 200,
    headers: { "Content-Type": "application/json" },
  });
}

describe("Comentarios", () => {
  afterEach(() => {
    cleanup();
    vi.unstubAllGlobals();
  });

  it("lista os comentarios aprovados retornados pelo BFF", async () => {
    vi.stubGlobal(
      "fetch",
      vi.fn().mockResolvedValue(
        respostaJson([
          {
            id: "c1",
            artigoId: "a1",
            artigoSlug: "meu-artigo",
            autor: "Ana",
            texto: "Muito bom",
            status: "Aprovado",
            dataCriacao: "2026-01-15T12:00:00Z",
          },
        ]),
      ),
    );

    render(<Comentarios artigoId="a1" artigoSlug="meu-artigo" />);

    await waitFor(() => expect(screen.getByText("Ana")).toBeTruthy());
    expect(screen.getByText("Muito bom")).toBeTruthy();
  });

  it("mostra convite quando nao ha comentarios", async () => {
    vi.stubGlobal("fetch", vi.fn().mockResolvedValue(respostaJson([])));

    render(<Comentarios artigoId="a1" artigoSlug="meu-artigo" />);

    await waitFor(() => expect(screen.getByText(/Seja o primeiro/i)).toBeTruthy());
  });
});
