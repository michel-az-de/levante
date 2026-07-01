import { afterEach, describe, expect, it } from "vitest";
import { definirToken, limparToken, obterToken } from "@/lib/auth";

describe("auth (armazenamento do token do admin)", () => {
  afterEach(() => {
    window.localStorage.clear();
  });

  it("obterToken retorna null quando nada foi salvo", () => {
    expect(obterToken()).toBeNull();
  });

  it("definirToken persiste e obterToken devolve o mesmo valor", () => {
    definirToken("jwt-de-teste");
    expect(obterToken()).toBe("jwt-de-teste");
  });

  it("limparToken remove o token salvo", () => {
    definirToken("jwt-de-teste");
    limparToken();
    expect(obterToken()).toBeNull();
  });
});
