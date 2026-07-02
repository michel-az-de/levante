/**
 * Cookie de visitante anonimo (first-party), gerido pelo BFF publico. E um id
 * opaco usado para deduplicar reacoes/comentarios no servidor — nao identifica
 * pessoa nem rastreia entre sites (SameSite=Lax, httpOnly). Ver LGPD no README.
 */
export const COOKIE_VISITANTE = "lev_vid";

/** Validade do cookie de visitante (1 ano). */
export const VISITANTE_MAX_AGE_SEGUNDOS = 60 * 60 * 24 * 365;

/** Header com que o BFF repassa o id de visitante para a API. */
export const HEADER_VISITANTE = "X-Visitante";
