import { cleanup, fireEvent, render, screen, waitFor } from "@testing-library/react";
import { afterEach, describe, expect, it, vi } from "vitest";
import { FormComentario } from "@/components/FormComentario";

describe("FormComentario", () => {
  afterEach(() => {
    cleanup();
    vi.unstubAllGlobals();
  });

  it("envia autor/texto/slug (+ honeypot) e mostra aviso de moderacao no 202", async () => {
    const fetchMock = vi.fn().mockResolvedValue(new Response(null, { status: 202 }));
    vi.stubGlobal("fetch", fetchMock);

    render(<FormComentario artigoId="a1" artigoSlug="meu-artigo" />);

    fireEvent.change(screen.getByLabelText("Seu nome"), { target: { value: "Ana" } });
    fireEvent.change(screen.getByLabelText("Comentario"), { target: { value: "Otimo!" } });
    fireEvent.click(screen.getByRole("button", { name: "Comentar" }));

    await waitFor(() => expect(screen.getByText(/aprovado na moderacao/i)).toBeTruthy());

    const [url, init] = fetchMock.mock.calls[0] as [string, RequestInit];
    expect(url).toBe("/api/publico/artigos/a1/comentarios");
    expect(init.method).toBe("POST");
    expect(JSON.parse(init.body as string)).toEqual({
      artigoSlug: "meu-artigo",
      autor: "Ana",
      texto: "Otimo!",
      armadilha: "",
    });
  });

  it("em 400 mostra erro e nao a mensagem de sucesso", async () => {
    vi.stubGlobal("fetch", vi.fn().mockResolvedValue(new Response(null, { status: 400 })));

    render(<FormComentario artigoId="a1" artigoSlug="meu-artigo" />);
    fireEvent.change(screen.getByLabelText("Seu nome"), { target: { value: "Ana" } });
    fireEvent.change(screen.getByLabelText("Comentario"), { target: { value: "x" } });
    fireEvent.click(screen.getByRole("button", { name: "Comentar" }));

    await waitFor(() => expect(screen.getByText(/Verifique o nome/i)).toBeTruthy());
    expect(screen.queryByText(/aprovado na moderacao/i)).toBeNull();
  });
});
