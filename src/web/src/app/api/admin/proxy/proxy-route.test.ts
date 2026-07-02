import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";

const cookieStore = { get: vi.fn() };
vi.mock("next/headers", () => ({
  cookies: () => Promise.resolve(cookieStore),
}));

import { GET, POST } from "@/app/api/admin/proxy/[...caminho]/route";

function contexto(...caminho: string[]) {
  return { params: Promise.resolve({ caminho }) };
}

describe("BFF /api/admin/proxy/[...caminho]", () => {
  beforeEach(() => {
    cookieStore.get.mockReset();
  });

  afterEach(() => {
    vi.unstubAllGlobals();
  });

  it("sem cookie de sessao responde 401 sem tocar a API", async () => {
    cookieStore.get.mockReturnValue(undefined);
    const fetchMock = vi.fn();
    vi.stubGlobal("fetch", fetchMock);

    const resposta = await GET(
      new Request("http://localhost:3000/api/admin/proxy/admin/artigos"),
      contexto("admin", "artigos"),
    );

    expect(resposta.status).toBe(401);
    expect(fetchMock).not.toHaveBeenCalled();
  });

  it("com cookie repassa para a API com Authorization: Bearer", async () => {
    cookieStore.get.mockReturnValue({ value: "jwt-abc" });
    const fetchMock = vi
      .fn()
      .mockResolvedValue(new Response("[]", { status: 200, headers: { "Content-Type": "application/json" } }));
    vi.stubGlobal("fetch", fetchMock);

    const resposta = await GET(
      new Request("http://localhost:3000/api/admin/proxy/admin/artigos"),
      contexto("admin", "artigos"),
    );

    expect(resposta.status).toBe(200);
    const [destino, init] = fetchMock.mock.calls[0] as [string, RequestInit];
    expect(destino.endsWith("/admin/artigos")).toBe(true);
    expect(new Headers(init.headers).get("authorization")).toBe("Bearer jwt-abc");
  });

  it("mutacao de origem cruzada vira 403 mesmo com cookie", async () => {
    cookieStore.get.mockReturnValue({ value: "jwt-abc" });
    const fetchMock = vi.fn();
    vi.stubGlobal("fetch", fetchMock);

    const resposta = await POST(
      new Request("http://localhost:3000/api/admin/proxy/artigos", {
        method: "POST",
        headers: { origin: "https://malicioso.exemplo" },
      }),
      contexto("artigos"),
    );

    expect(resposta.status).toBe(403);
    expect(fetchMock).not.toHaveBeenCalled();
  });
});
