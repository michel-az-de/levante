/**
 * Constantes da sessao do admin no BFF (route handlers + testes).
 * O valor do cookie e o JWT bearer da API; httpOnly = invisivel ao JS.
 */
export const COOKIE_SESSAO_ADMIN = "levante_admin_sessao";

/** Fallback quando a API nao informa expiracao (15 min, igual ao JWT). */
export const SESSAO_MAX_AGE_PADRAO_SEGUNDOS = 900;

/** URL base da API no SERVIDOR (env privada; o browser nunca chama a API direto no admin). */
export function apiBaseUrl(): string {
  return process.env.API_BASE_URL ?? "http://localhost:5080";
}
