import createClient, { type Middleware } from "openapi-fetch";
import type { paths } from "@/types/api";

// Cliente do admin (browser): guarda o JWT em localStorage e injeta no header.
// Trade-off conhecido (escolha de JWT bearer): exposto a XSS. TODO de hardening:
// migrar para cookie httpOnly/BFF.
const CHAVE_TOKEN = "levante.admin.token";
const baseUrl = process.env.NEXT_PUBLIC_API_BASE_URL ?? "http://localhost:5080";

export function obterToken(): string | null {
  if (typeof window === "undefined") {
    return null;
  }
  return window.localStorage.getItem(CHAVE_TOKEN);
}

export function definirToken(token: string): void {
  window.localStorage.setItem(CHAVE_TOKEN, token);
}

export function limparToken(): void {
  window.localStorage.removeItem(CHAVE_TOKEN);
}

const autorizacao: Middleware = {
  onRequest({ request }) {
    const token = obterToken();
    if (token) {
      request.headers.set("Authorization", `Bearer ${token}`);
    }
    return request;
  },
};

export const apiAdmin = createClient<paths>({ baseUrl });
apiAdmin.use(autorizacao);
