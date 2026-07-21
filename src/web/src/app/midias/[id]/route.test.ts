import { afterEach, describe, expect, it, vi } from "vitest";

// Isola o handler do resto do modulo de sessao (cookies/next-headers): so precisa da base da API.
vi.mock("@/lib/sessao-admin", () => ({
  apiBaseUrl: () => "http://api.test",
}));

import { GET } from "@/app/midias/[id]/route";

function contexto(id: string) {
  return { params: Promise.resolve({ id }) };
}

describe("BFF publico /midias/[id]", () => {
  afterEach(() => vi.unstubAllGlobals());

  it("repassa status, corpo e so os cabecalhos da lista (inclui nosniff, descarta o resto)", async () => {
    const fetchMock = vi.fn().mockResolvedValue(
      new Response("bytes-da-imagem", {
        status: 200,
        headers: {
          "content-type": "image/png",
          "cache-control": "public, max-age=31536000, immutable",
          etag: '"abc"',
          "content-length": "15",
          "x-content-type-options": "nosniff",
          "x-segredo": "nao-deve-vazar",
        },
      }),
    );
    vi.stubGlobal("fetch", fetchMock);

    const resposta = await GET(new Request("http://localhost:3000/midias/abc"), contexto("abc"));

    expect(resposta.status).toBe(200);
    expect(resposta.headers.get("content-type")).toBe("image/png");
    expect(resposta.headers.get("cache-control")).toContain("immutable");
    expect(resposta.headers.get("etag")).toBe('"abc"');
    expect(resposta.headers.get("content-length")).toBe("15");
    expect(resposta.headers.get("x-content-type-options")).toBe("nosniff");
    expect(resposta.headers.get("x-segredo")).toBeNull(); // fora da lista de repasse
    expect(await resposta.text()).toBe("bytes-da-imagem");

    const [destino] = fetchMock.mock.calls[0] as [string];
    expect(destino).toBe("http://api.test/midias/abc");
  });

  it("encaminha If-None-Match e devolve 304 quando a API responde 304", async () => {
    const fetchMock = vi.fn().mockResolvedValue(new Response(null, { status: 304, headers: { etag: '"abc"' } }));
    vi.stubGlobal("fetch", fetchMock);

    const resposta = await GET(
      new Request("http://localhost:3000/midias/abc", { headers: { "if-none-match": '"abc"' } }),
      contexto("abc"),
    );

    expect(resposta.status).toBe(304);
    const [, init] = fetchMock.mock.calls[0] as [string, RequestInit];
    expect(new Headers(init.headers).get("if-none-match")).toBe('"abc"');
  });

  it("propaga X-Forwarded-For para o rate limit por IP da API", async () => {
    const fetchMock = vi.fn().mockResolvedValue(new Response("x", { status: 200 }));
    vi.stubGlobal("fetch", fetchMock);

    await GET(
      new Request("http://localhost:3000/midias/abc", { headers: { "x-forwarded-for": "203.0.113.7" } }),
      contexto("abc"),
    );

    const [, init] = fetchMock.mock.calls[0] as [string, RequestInit];
    expect(new Headers(init.headers).get("x-forwarded-for")).toBe("203.0.113.7");
  });

  it("falha/timeout da API vira 502 com corpo vazio (nao HTML de erro dentro de <img>)", async () => {
    const fetchMock = vi.fn().mockRejectedValue(new Error("timeout"));
    vi.stubGlobal("fetch", fetchMock);

    const resposta = await GET(new Request("http://localhost:3000/midias/abc"), contexto("abc"));

    expect(resposta.status).toBe(502);
    expect(await resposta.text()).toBe("");
  });
});
