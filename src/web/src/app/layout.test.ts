import { describe, expect, it } from "vitest";
import { dynamic } from "./layout";

describe("RootLayout", () => {
  // Pagina pre-renderizada no build assaria o fallback localhost no canonical e no OG (#177).
  it("renderiza todas as rotas no request para ler o SITE_URL do container", () => {
    expect(dynamic).toBe("force-dynamic");
  });
});
