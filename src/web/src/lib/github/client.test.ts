import { afterEach, describe, expect, it, vi } from "vitest";
import { githubGraphQL, githubRest } from "@/lib/github/client";

describe("cliente GitHub", () => {
  afterEach(() => {
    vi.unstubAllGlobals();
  });

  it("githubRest envia Authorization quando ha token e retorna o JSON", async () => {
    const fetchMock = vi
      .fn()
      .mockResolvedValue(new Response(JSON.stringify({ ok: 1 }), { status: 200 }));
    vi.stubGlobal("fetch", fetchMock);

    const dado = await githubRest<{ ok: number }>("/repos/x/y", 3600, "tok");

    expect(dado.ok).toBe(1);
    const [url, init] = fetchMock.mock.calls[0] as [string, RequestInit];
    expect(url).toBe("https://api.github.com/repos/x/y");
    expect(new Headers(init.headers).get("Authorization")).toBe("Bearer tok");
  });

  it("githubRest omite Authorization sem token e lanca em status != 2xx", async () => {
    const fetchMock = vi.fn().mockResolvedValue(new Response("nope", { status: 403 }));
    vi.stubGlobal("fetch", fetchMock);

    await expect(githubRest("/x", 3600, undefined)).rejects.toThrow(/403/);
    const [, init] = fetchMock.mock.calls[0] as [string, RequestInit];
    expect(new Headers(init.headers).get("Authorization")).toBeNull();
  });

  it("githubGraphQL faz POST com a query e lanca quando ha errors", async () => {
    const fetchMock = vi
      .fn()
      .mockResolvedValue(new Response(JSON.stringify({ errors: [{ message: "x" }] }), { status: 200 }));
    vi.stubGlobal("fetch", fetchMock);

    await expect(githubGraphQL("query{}", {}, 86400, "tok")).rejects.toThrow(/errors/);
    const [url, init] = fetchMock.mock.calls[0] as [string, RequestInit];
    expect(url).toBe("https://api.github.com/graphql");
    expect(init.method).toBe("POST");
  });

  it("githubGraphQL retorna data quando a resposta e valida", async () => {
    const fetchMock = vi
      .fn()
      .mockResolvedValue(new Response(JSON.stringify({ data: { valor: 42 } }), { status: 200 }));
    vi.stubGlobal("fetch", fetchMock);

    const dados = await githubGraphQL<{ valor: number }>("query{}", {}, 86400, "tok");
    expect(dados.valor).toBe(42);
  });
});
