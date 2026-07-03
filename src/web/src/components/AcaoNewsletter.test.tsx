import { cleanup, render, screen, waitFor } from "@testing-library/react";
import { afterEach, describe, expect, it, vi } from "vitest";
import { AcaoNewsletter } from "@/components/AcaoNewsletter";

describe("AcaoNewsletter", () => {
  afterEach(() => {
    cleanup();
    vi.unstubAllGlobals();
  });

  it("confirma: POST /api/publico/newsletter/confirmar com o token e mostra sucesso", async () => {
    const fetchMock = vi.fn().mockResolvedValue(new Response(null, { status: 200 }));
    vi.stubGlobal("fetch", fetchMock);

    render(<AcaoNewsletter token="tok-123" acao="confirmar" />);

    await waitFor(() => expect(screen.getByText(/Inscricao confirmada/i)).toBeTruthy());

    const [url, init] = fetchMock.mock.calls[0] as [string, RequestInit];
    expect(url).toBe("/api/publico/newsletter/confirmar");
    expect(init.method).toBe("POST");
    expect(JSON.parse(init.body as string)).toEqual({ token: "tok-123" });
  });

  it("cancela: mostra a mensagem de cancelamento no sucesso", async () => {
    vi.stubGlobal("fetch", vi.fn().mockResolvedValue(new Response(null, { status: 200 })));

    render(<AcaoNewsletter token="tok-9" acao="cancelar" />);

    await waitFor(() => expect(screen.getByText(/Inscricao cancelada/i)).toBeTruthy());
  });

  it("em 404 mostra erro de link invalido", async () => {
    vi.stubGlobal("fetch", vi.fn().mockResolvedValue(new Response(null, { status: 404 })));

    render(<AcaoNewsletter token="tok-x" acao="confirmar" />);

    await waitFor(() => expect(screen.getByText(/Link invalido/i)).toBeTruthy());
  });

  it("sem token nao chama a API e mostra erro", async () => {
    const fetchMock = vi.fn();
    vi.stubGlobal("fetch", fetchMock);

    render(<AcaoNewsletter token="" acao="confirmar" />);

    await waitFor(() => expect(screen.getByText(/Link invalido/i)).toBeTruthy());
    expect(fetchMock).not.toHaveBeenCalled();
  });
});
