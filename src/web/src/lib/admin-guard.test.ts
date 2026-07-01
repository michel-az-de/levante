import { afterEach, describe, expect, it, vi } from "vitest";
import { definirToken, obterToken } from "@/lib/auth";
import { tratarNaoAutorizado } from "@/lib/admin-guard";

describe("tratarNaoAutorizado (401 do servidor)", () => {
  afterEach(() => {
    window.localStorage.clear();
  });

  it("em 401 limpa o token e redireciona para o login", () => {
    definirToken("jwt-expirado");
    const replace = vi.fn();

    const tratado = tratarNaoAutorizado(401, { replace });

    expect(tratado).toBe(true);
    expect(obterToken()).toBeNull();
    expect(replace).toHaveBeenCalledWith("/admin/login");
  });

  it("em outros status nao mexe no token nem redireciona", () => {
    definirToken("jwt-valido");
    const replace = vi.fn();

    const tratado = tratarNaoAutorizado(500, { replace });

    expect(tratado).toBe(false);
    expect(obterToken()).toBe("jwt-valido");
    expect(replace).not.toHaveBeenCalled();
  });
});
