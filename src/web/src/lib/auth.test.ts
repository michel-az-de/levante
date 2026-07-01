import { afterEach, describe, expect, it, vi } from "vitest";
import { entrarComoAdmin, sairDoAdmin } from "@/lib/auth";

describe("auth (sessao do admin via BFF)", () => {
  afterEach(() => {
    vi.unstubAllGlobals();
  });

  it("entrarComoAdmin envia credenciais ao BFF e retorna true no 204", async () => {
    const fetchMock = vi.fn().mockResolvedValue(new Response(null, { status: 204 }));
    vi.stubGlobal("fetch", fetchMock);

    const ok = await entrarComoAdmin("admin@levante.dev", "senha-forte");

    expect(ok).toBe(true);
    const [url, init] = fetchMock.mock.calls[0] as [string, RequestInit];
    expect(url).toBe("/api/admin/sessao");
    expect(init.method).toBe("POST");
    expect(JSON.parse(init.body as string)).toEqual({
      email: "admin@levante.dev",
      senha: "senha-forte",
    });
  });

  it("entrarComoAdmin retorna false quando o BFF responde 401", async () => {
    vi.stubGlobal("fetch", vi.fn().mockResolvedValue(new Response(null, { status: 401 })));

    expect(await entrarComoAdmin("admin@levante.dev", "senha-errada")).toBe(false);
  });

  it("sairDoAdmin chama DELETE /api/admin/sessao", async () => {
    const fetchMock = vi.fn().mockResolvedValue(new Response(null, { status: 204 }));
    vi.stubGlobal("fetch", fetchMock);

    await sairDoAdmin();

    expect(fetchMock).toHaveBeenCalledWith("/api/admin/sessao", { method: "DELETE" });
  });
});
