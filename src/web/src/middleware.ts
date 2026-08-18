import { NextResponse, type NextRequest } from "next/server";

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

/**
 * Headers de seguranca de toda resposta (paginas e handlers). Extraido do
 * middleware para ser testavel com um Headers puro. A CSP acima segue em
 * Report-Only; o anti-clickjacking aplicado de verdade e o X-Frame-Options.
 */
export function aplicarHeadersDeSeguranca(pathname: string, headers: Headers): void {
  headers.set("Content-Security-Policy-Report-Only", CSP_REPORT_ONLY);
  headers.set("X-Content-Type-Options", "nosniff");
  headers.set("X-Frame-Options", "DENY");
  headers.set("Referrer-Policy", "strict-origin-when-cross-origin");
  headers.set("Permissions-Policy", "camera=(), microphone=(), geolocation=(), payment=()");

  // HSTS so em producao: a borda (Caddy, TLS-only) nao emite HSTS por default e
  // em dev (http://localhost) o header nao pode existir. 180 dias, sem preload.
  if (process.env.NODE_ENV === "production") {
    headers.set("Strict-Transport-Security", "max-age=15552000; includeSubDomains");
  }

  // Enquanto SITE_INDEXABLE nao for "true" (host provisorio / pre-cutover), nao
  // indexar nada: a mesma app serve o host interino e o dominio final (complementa
  // o robots Disallow + sitemap vazio). O /admin nunca e indexavel, nem no dominio
  // final — o robots.ts ja o exclui, e o X-Robots-Tag pega o bot que ignora robots.
  if (pathname.startsWith("/admin") || process.env.SITE_INDEXABLE !== "true") {
    headers.set("X-Robots-Tag", "noindex, nofollow");
  }
}

export function middleware(request: NextRequest): NextResponse {
  const response = NextResponse.next();
  aplicarHeadersDeSeguranca(request.nextUrl.pathname, response.headers);
  return response;
}

export const config = {
  // Aplica a paginas e handlers; exclui os assets estaticos do Next.
  matcher: ["/((?!_next/static|_next/image|favicon.ico).*)"],
};
