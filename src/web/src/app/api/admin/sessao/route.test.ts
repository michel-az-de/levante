import { afterEach, describe, expect, it, vi } from "vitest";
import { DELETE, POST } from "@/app/api/admin/sessao/route";
import { COOKIE_SESSAO_ADMIN } from "@/lib/sessao-admin";

function requisicaoLogin(origin?: string, encaminhado?: string) {
  return new Request("http://localhost:3000/api/admin/sessao", {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
      ...(origin ? { origin } : {}),
      ...(encaminhado ? { "x-forwarded-for": encaminhado } : {}),
    },
    body: JSON.stringify({ email: "admin@levante.dev", senha: "senha" }),
  });
}

describe("BFF /api/admin/sessao", () => {
  afterEach(() => {
    vi.unstubAllGlobals();
  });

  it("login valido vira 204 com cookie httpOnly/sameSite=strict do JWT", async () => {
    vi.stubGlobal(
      "fetch",
      vi.fn().mockResolvedValue(
        new Response(JSON.stringify({ accessToken: "jwt-abc", expiraEmSegundos: 600 }), {
          status: 200,
          headers: { "Content-Type": "application/json" },
        }),
      ),
    );

    const resposta = await POST(requisicaoLogin());

    expect(resposta.status).toBe(204);
    const cookie = resposta.headers.get("set-cookie") ?? "";
    expect(cookie).toContain(`${COOKIE_SESSAO_ADMIN}=jwt-abc`);
    expect(cookie.toLowerCase()).toContain("httponly");
    expect(cookie.toLowerCase()).toContain("samesite=strict");
    expect(cookie.toLowerCase()).toContain("max-age=600");
  });

  it("credenciais invalidas viram 401 sem cookie", async () => {
    vi.stubGlobal("fetch", vi.fn().mockResolvedValue(new Response(null, { status: 401 })));

    const resposta = await POST(requisicaoLogin());

    expect(resposta.status).toBe(401);
    expect(resposta.headers.get("set-cookie")).toBeNull();
  });

  it("propaga X-Forwarded-For para o rate limit do /auth/login", async () => {
    // Sem isto os 5 req/min do PolicyAuth viram um balde unico (o IP do container do Next):
    // qualquer um queima as tentativas e tranca o admin legitimo (ver issue #93).
    const fetchMock = vi.fn().mockResolvedValue(new Response(null, { status: 401 }));
    vi.stubGlobal("fetch", fetchMock);

    await POST(requisicaoLogin(undefined, "203.0.113.7"));

    const [destino, init] = fetchMock.mock.calls[0] as [string, RequestInit];
    expect(destino.endsWith("/auth/login")).toBe(true);
    expect(new Headers(init.headers).get("x-forwarded-for")).toBe("203.0.113.7");
    expect(new Headers(init.headers).get("content-type")).toBe("application/json");
  });

  it("origem cruzada no login vira 403 (anti-CSRF)", async () => {
    const resposta = await POST(requisicaoLogin("https://malicioso.exemplo"));

    expect(resposta.status).toBe(403);
  });

  it("logout expira o cookie", () => {
    const resposta = DELETE(new Request("http://localhost:3000/api/admin/sessao", { method: "DELETE" }));

    expect(resposta.status).toBe(204);
    const cookie = (resposta.headers.get("set-cookie") ?? "").toLowerCase();
    expect(cookie).toContain(`${COOKIE_SESSAO_ADMIN.toLowerCase()}=`);
    expect(cookie).toContain("max-age=0");
  });
});
