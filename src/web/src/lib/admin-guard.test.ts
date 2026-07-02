import { describe, expect, it, vi } from "vitest";
import { tratarNaoAutorizado } from "@/lib/admin-guard";

describe("tratarNaoAutorizado (401 do servidor)", () => {
  it("em 401 redireciona para o login", () => {
    const replace = vi.fn();

    const tratado = tratarNaoAutorizado(401, { replace });

    expect(tratado).toBe(true);
    expect(replace).toHaveBeenCalledWith("/admin/login");
  });

  it("em outros status nao redireciona", () => {
    const replace = vi.fn();

    const tratado = tratarNaoAutorizado(500, { replace });

    expect(tratado).toBe(false);
    expect(replace).not.toHaveBeenCalled();
  });
});
