import { beforeEach, describe, expect, it, vi } from "vitest";
import { enviarMidia } from "@/lib/midias";

describe("enviarMidia", () => {
  beforeEach(() => {
    vi.unstubAllGlobals();
  });

  it("envia o arquivo como multipart e retorna a midia criada", async () => {
    const respostaFalsa = {
      id: "11111111-1111-1111-1111-111111111111",
      url: "/midias/11111111-1111-1111-1111-111111111111",
      contentType: "image/png",
      tamanho: 123,
    };
    const fetchMock = vi
      .fn()
      .mockResolvedValue(
        new Response(JSON.stringify(respostaFalsa), {
          status: 201,
          headers: { "Content-Type": "application/json" },
        }),
      );
    vi.stubGlobal("fetch", fetchMock);

    const arquivo = new File([new Uint8Array([1, 2, 3])], "foto.png", { type: "image/png" });
    const midia = await enviarMidia(arquivo);

    expect(midia).toEqual(respostaFalsa);
    expect(fetchMock).toHaveBeenCalledTimes(1);

    const [, init] = fetchMock.mock.calls[0] as [string, RequestInit];
    expect(init.body).toBeInstanceOf(FormData);
    expect((init.body as FormData).get("arquivo")).toBe(arquivo);
  });

  it("lanca erro quando a API recusa o upload", async () => {
    vi.stubGlobal(
      "fetch",
      vi.fn().mockResolvedValue(
        new Response(JSON.stringify({ title: "Validacao" }), {
          status: 400,
          headers: { "Content-Type": "application/problem+json" },
        }),
      ),
    );

    const arquivo = new File([new Uint8Array([1])], "ruim.pdf", { type: "application/pdf" });

    await expect(enviarMidia(arquivo)).rejects.toThrow(/400/);
  });
});
