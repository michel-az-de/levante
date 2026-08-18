import { afterEach, describe, expect, it, vi } from "vitest";
import { aplicarHeadersDeSeguranca } from "@/middleware";

function headersPara(pathname: string): Headers {
  const headers = new Headers();
  aplicarHeadersDeSeguranca(pathname, headers);
  return headers;
}

describe("aplicarHeadersDeSeguranca", () => {
  afterEach(() => {
    vi.unstubAllEnvs();
  });

  it("aplica os headers base em qualquer rota", () => {
    const headers = headersPara("/");

    expect(headers.get("X-Content-Type-Options")).toBe("nosniff");
    expect(headers.get("X-Frame-Options")).toBe("DENY");
    expect(headers.get("Referrer-Policy")).toBe("strict-origin-when-cross-origin");
    expect(headers.get("Permissions-Policy")).toContain("camera=()");
    expect(headers.get("Content-Security-Policy-Report-Only")).toContain("frame-ancestors 'none'");
  });

  it("HSTS so existe em producao", () => {
    expect(headersPara("/").get("Strict-Transport-Security")).toBeNull();

    vi.stubEnv("NODE_ENV", "production");
    expect(headersPara("/").get("Strict-Transport-Security")).toContain("max-age=");
  });

  it("com o site indexavel, /admin continua noindex e o resto fica limpo", () => {
    vi.stubEnv("SITE_INDEXABLE", "true");

    expect(headersPara("/admin/artigos").get("X-Robots-Tag")).toBe("noindex, nofollow");
    expect(headersPara("/artigos").get("X-Robots-Tag")).toBeNull();
  });

  it("pre-cutover (flag ausente) tudo e noindex", () => {
    vi.stubEnv("SITE_INDEXABLE", "");

    expect(headersPara("/").get("X-Robots-Tag")).toBe("noindex, nofollow");
  });
});
