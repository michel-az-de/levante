// Flags de operacao lidas do ambiente do SERVIDOR (nunca NEXT_PUBLIC_). Lidas por
// funcao para valerem em runtime (route handlers force-dynamic, server components,
// middleware) — o cutover D0 e restart, nao rebuild.

/**
 * Indexacao por buscadores. Off por default: o host provisorio (sslip.io) NAO deve
 * ser indexado; so o dominio final (pos-marco D0) indexa. Setar SITE_INDEXABLE=true
 * no cutover. Flag explicito em vez de inferir por host: na fase provisoria o
 * SITE_URL ja e o proprio host interino, entao "Host != SITE_URL" nao detectaria.
 */
export function siteIndexavel(): boolean {
  return process.env.SITE_INDEXABLE === "true";
}

/**
 * Form publico de inscricao na newsletter. Off por default: ativar so apos o marco D0
 * e o provedor de e-mail (Resend) com dominio de envio verificado — senao o
 * confirmUrlBase apontaria para o host provisorio e/ou queimaria a reputacao do
 * dominio novo. As paginas de confirmar/cancelar seguem valendo (tokens existentes).
 */
export function newsletterHabilitada(): boolean {
  return process.env.NEWSLETTER_ENABLED === "true";
}
