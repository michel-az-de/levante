import { cleanup, fireEvent, render, screen, waitFor } from "@testing-library/react";
import { afterEach, describe, expect, it, vi } from "vitest";
import { ReacoesArtigo } from "@/components/ReacoesArtigo";

function respostaJson(dados: unknown): Response {
  return new Response(JSON.stringify(dados), {
    status: 200,
    headers: { "Content-Type": "application/json" },
  });
}

function botaoCurtir(): HTMLElement {
  return screen.getByRole("button", { name: /Curtir/ });
}

describe("ReacoesArtigo", () => {
  afterEach(() => {
    cleanup();
    vi.unstubAllGlobals();
  });

  it("carrega contagens e, ao curtir, dispara POST e atualiza", async () => {
    const fetchMock = vi
      .fn()
      .mockResolvedValueOnce(respostaJson({ curtir: 2, amei: 0, relevante: 1, minhas: [] }))
      .mockResolvedValueOnce(respostaJson({ curtir: 3, amei: 0, relevante: 1, minhas: ["Curtir"] }));
    vi.stubGlobal("fetch", fetchMock);

    render(<ReacoesArtigo artigoId="a1" />);

    await waitFor(() => expect(botaoCurtir().textContent).toContain("2"));

    fireEvent.click(botaoCurtir());

    await waitFor(() => expect(botaoCurtir().textContent).toContain("3"));
    expect(botaoCurtir().getAttribute("aria-pressed")).toBe("true");

    const [url, init] = fetchMock.mock.calls[1] as [string, RequestInit];
    expect(url).toBe("/api/publico/artigos/a1/reacoes");
    expect(init.method).toBe("POST");
    expect(JSON.parse(init.body as string)).toEqual({ tipo: "Curtir" });
  });

  it("clicar numa reacao ja ativa dispara DELETE", async () => {
    const fetchMock = vi
      .fn()
      .mockResolvedValueOnce(respostaJson({ curtir: 1, amei: 0, relevante: 0, minhas: ["Curtir"] }))
      .mockResolvedValueOnce(respostaJson({ curtir: 0, amei: 0, relevante: 0, minhas: [] }));
    vi.stubGlobal("fetch", fetchMock);

    render(<ReacoesArtigo artigoId="a1" />);

    await waitFor(() => expect(botaoCurtir().getAttribute("aria-pressed")).toBe("true"));

    fireEvent.click(botaoCurtir());

    await waitFor(() => expect(fetchMock).toHaveBeenCalledTimes(2));
    const [url, init] = fetchMock.mock.calls[1] as [string, RequestInit];
    expect(url).toBe("/api/publico/artigos/a1/reacoes/Curtir");
    expect(init.method).toBe("DELETE");
  });
});
