import { cleanup, fireEvent, render, screen, waitFor } from "@testing-library/react";
import { afterEach, describe, expect, it, vi } from "vitest";
import { FormInscricaoNewsletter } from "@/components/FormInscricaoNewsletter";

describe("FormInscricaoNewsletter", () => {
  afterEach(() => {
    cleanup();
    vi.unstubAllGlobals();
  });

  it("envia email (+ honeypot) para /api/publico/newsletter e mostra aviso de confirmacao", async () => {
    const fetchMock = vi.fn().mockResolvedValue(new Response(null, { status: 202 }));
    vi.stubGlobal("fetch", fetchMock);

    render(<FormInscricaoNewsletter />);

    fireEvent.change(screen.getByLabelText("Seu e-mail"), { target: { value: "ana@exemplo.com" } });
    fireEvent.click(screen.getByRole("button", { name: "Inscrever" }));

    await waitFor(() => expect(screen.getByText(/e-mail de confirmacao/i)).toBeTruthy());

    const [url, init] = fetchMock.mock.calls[0] as [string, RequestInit];
    expect(url).toBe("/api/publico/newsletter");
    expect(init.method).toBe("POST");
    expect(JSON.parse(init.body as string)).toEqual({ email: "ana@exemplo.com", armadilha: "" });
  });

  it("em 400 mostra erro e nao a mensagem de sucesso", async () => {
    vi.stubGlobal("fetch", vi.fn().mockResolvedValue(new Response(null, { status: 400 })));

    render(<FormInscricaoNewsletter />);
    fireEvent.change(screen.getByLabelText("Seu e-mail"), { target: { value: "x@y" } });
    fireEvent.click(screen.getByRole("button", { name: "Inscrever" }));

    await waitFor(() => expect(screen.getByText(/Verifique o e-mail/i)).toBeTruthy());
    expect(screen.queryByText(/e-mail de confirmacao/i)).toBeNull();
  });
});
