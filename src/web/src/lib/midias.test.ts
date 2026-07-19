import { beforeEach, describe, expect, it, vi } from "vitest";
import { enviarMidia } from "@/lib/midias";

// apiAdmin e um singleton criado na importacao de @/lib/auth (openapi-fetch
// captura o fetch global naquele momento, cedo demais para vi.stubGlobal
// alcancar); mockar o modulo inteiro evita depender de fetch/URL relativa
// (que quebra fora do browser - o erro real visto no CI era o fetch nativo
// do Node rejeitando "/api/admin/proxy/admin/midias" por nao ter origem).
// vi.hoisted: a fabrica do vi.mock roda hoisted para o topo do arquivo (antes
// de qualquer const), entao postMock so pode ser referenciado ali se tambem
// for hoisted.
const { postMock } = vi.hoisted(() => ({ postMock: vi.fn() }));

vi.mock("@/lib/auth", () => ({
  apiAdmin: { POST: (...args: unknown[]) => postMock(...args) },
}));

describe("enviarMidia", () => {
  beforeEach(() => {
    postMock.mockReset();
  });

  it("chama POST /admin/midias com o arquivo como multipart e retorna a midia criada", async () => {
    const respostaFalsa = {
      id: "11111111-1111-1111-1111-111111111111",
      url: "/midias/11111111-1111-1111-1111-111111111111",
      contentType: "image/png",
      tamanho: 123,
    };
    postMock.mockResolvedValue({ data: respostaFalsa, error: undefined, response: { status: 201 } });

    const arquivo = new File([new Uint8Array([1, 2, 3])], "foto.png", { type: "image/png" });
    const midia = await enviarMidia(arquivo);

    expect(midia).toEqual(respostaFalsa);
    expect(postMock).toHaveBeenCalledTimes(1);

    const [caminho, opcoes] = postMock.mock.calls[0] as [string, { body: unknown; bodySerializer: (b: unknown) => FormData }];
    expect(caminho).toBe("/admin/midias");

    const formulario = opcoes.bodySerializer(opcoes.body);
    expect(formulario).toBeInstanceOf(FormData);
    expect(formulario.get("arquivo")).toBe(arquivo);
  });

  it("lanca erro quando a API recusa o upload", async () => {
    postMock.mockResolvedValue({
      data: undefined,
      error: { title: "Validacao" },
      response: { status: 400 },
    });

    const arquivo = new File([new Uint8Array([1])], "ruim.pdf", { type: "application/pdf" });

    await expect(enviarMidia(arquivo)).rejects.toThrow(/400/);
  });
});
