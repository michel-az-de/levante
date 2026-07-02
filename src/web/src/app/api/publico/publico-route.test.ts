import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import { COOKIE_VISITANTE, HEADER_VISITANTE } from "@/lib/visitante";

const cookieStore = { get: vi.fn() };
vi.mock("next/headers", () => ({
  cookies: () => Promise.resolve(cookieStore),
}));

import { GET, POST } from "@/app/api/publico/[...caminho]/route";

function contexto(...caminho: string[]) {
  return { params: Promise.resolve({ caminho }) };
}

describe("BFF publico /api/publico/[...caminho]", () => {
  beforeEach(() => {
    cookieStore.get.mockReset();
  });

  afterEach(() => {
    vi.unstubAllGlobals();
  });

  it("repassa GET a API com o visitante do cookie no header X-Visitante", async () => {
    cookieStore.get.mockReturnValue({ value: "vid-123" });
    const fetchMock = vi
      .fn()
      .mockResolvedValue(new Response("{}", { status: 200, headers: { "Content-Type": "application/json" } }));
    vi.stubGlobal("fetch", fetchMock);

    const resposta = await GET(
      new Request("http://localhost:3000/api/publico/artigos/a1/reacoes"),
      contexto("artigos", "a1", "reacoes"),
    );

    expect(resposta.status).toBe(200);
    const [destino, init] = fetchMock.mock.calls[0] as [string, RequestInit];
    expect(destino.endsWith("/artigos/a1/reacoes")).toBe(true);
    expect(new Headers(init.headers).get(HEADER_VISITANTE)).toBe("vid-123");
  });

  it("sem cookie: gera um visitante, repassa no header e o define (httpOnly)", async () => {
    cookieStore.get.mockReturnValue(undefined);
    const fetchMock = vi.fn().mockResolvedValue(new Response(null, { status: 200 }));
    vi.stubGlobal("fetch", fetchMock);

    const resposta = await POST(
      new Request("http://localhost:3000/api/publico/artigos/a1/reacoes", {
        method: "POST",
        headers: { "content-type": "application/json" },
        body: JSON.stringify({ tipo: "Curtir" }),
      }),
      contexto("artigos", "a1", "reacoes"),
    );

    const cookie = (resposta.headers.get("set-cookie") ?? "").toLowerCase();
    expect(cookie).toContain(`${COOKIE_VISITANTE.toLowerCase()}=`);
    expect(cookie).toContain("httponly");

    const enviado = new Headers(fetchMock.mock.calls[0][1].headers).get(HEADER_VISITANTE);
    expect(enviado).toBeTruthy();
  });
});
