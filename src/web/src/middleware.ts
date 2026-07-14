import { NextResponse } from "next/server";

// CSP em Report-Only no MVP: nao bloqueia, so coleta violacoes em /api/csp-report
// para servir de evidencia antes do enforcement. Nonce/hash matam o cache ISR do
// blog (ver docs/plano-mvp-producao.md). A politica ja antecipa as fontes de E2/E3
// (imagens/avatares e API do GitHub).
const CSP_REPORT_ONLY = [
  "default-src 'self'",
  "base-uri 'self'",
  "object-src 'none'",
  "frame-ancestors 'none'",
  "form-action 'self'",
  "img-src 'self' data: https://*.githubusercontent.com",
  "font-src 'self' data:",
  "style-src 'self' 'unsafe-inline'",
  "script-src 'self' 'unsafe-inline'",
  "connect-src 'self' https://api.github.com",
  "report-uri /api/csp-report",
].join("; ");

export function middleware(): NextResponse {
  const response = NextResponse.next();
  response.headers.set("Content-Security-Policy-Report-Only", CSP_REPORT_ONLY);

  // Enquanto SITE_INDEXABLE nao for "true" (host provisorio / pre-cutover), nao
  // indexar: a mesma app serve o host interino e o dominio final. Complementa o
  // robots Disallow + sitemap vazio (X-Robots-Tag pega o bot que ignora o robots).
  if (process.env.SITE_INDEXABLE !== "true") {
    response.headers.set("X-Robots-Tag", "noindex, nofollow");
  }

  return response;
}

export const config = {
  // Aplica a paginas e handlers; exclui os assets estaticos do Next.
  matcher: ["/((?!_next/static|_next/image|favicon.ico).*)"],
};
