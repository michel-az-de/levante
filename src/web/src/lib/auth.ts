import createClient from "openapi-fetch";
import type { paths } from "@/types/api";

/**
 * Cliente do admin (browser) via BFF: as chamadas vao para /api/admin/proxy/*
 * no proprio Next, que anexa o JWT guardado em cookie httpOnly (o token nunca
 * fica acessivel ao JS — hardening anti-XSS executado; era TODO da Fatia 2a).
 * A sessao e criada/encerrada por /api/admin/sessao.
 */
export const apiAdmin = createClient<paths>({ baseUrl: "/api/admin/proxy" });

/** Cria a sessao do admin (cookie httpOnly). Retorna false para credenciais invalidas. */
export async function entrarComoAdmin(email: string, senha: string): Promise<boolean> {
  const resposta = await fetch("/api/admin/sessao", {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ email, senha }),
  });
  return resposta.ok;
}

/** Encerra a sessao do admin (expira o cookie). */
export async function sairDoAdmin(): Promise<void> {
  await fetch("/api/admin/sessao", { method: "DELETE" });
}
