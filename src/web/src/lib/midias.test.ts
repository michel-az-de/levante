import { beforeEach, describe, expect, it, vi } from "vitest";
import { enviarMidia, ErroDeMidia, removerMidia } from "@/lib/midias";

// apiAdmin e um singleton criado na importacao de @/lib/auth (openapi-fetch
// captura o fetch global naquele momento, cedo demais para vi.stubGlobal
// alcancar); mockar o modulo inteiro evita depender de fetch/URL relativa
// (que quebra fora do browser - o erro real visto no CI era o fetch nativo
// do Node rejeitando "/api/admin/proxy/admin/midias" por nao ter origem).
// vi.hoisted: a fabrica do vi.mock roda hoisted para o topo do arquivo (antes
// de qualquer const), entao os mocks so podem ser referenciados ali se tambem
// forem hoisted.
const { postMock, deleteMock } = vi.hoisted(() => ({
  postMock: vi.fn(),
  deleteMock: vi.fn(),
}));

vi.mock("@/lib/auth", () => ({
  apiAdmin: {
    POST: (...args: unknown[]) => postMock(...args),
    DELETE: (...args: unknown[]) => deleteMock(...args),
  },
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

    const [caminho, opcoes] = postMock.mock.calls[0] as [
      string,
      { body: unknown; bodySerializer: (b: unknown) => FormData },
    ];
    expect(caminho).toBe("/admin/midias");

    // O nome do campo precisa casar com MidiaAdminEndpoints.CampoArquivo no backend.
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

  it("propaga o status 413 (payload grande demais) na mensagem de erro", async () => {
    postMock.mockResolvedValue({
      data: undefined,
      error: { title: "Payload Too Large" },
      response: { status: 413 },
    });

    const arquivo = new File([new Uint8Array(10)], "grande.png", { type: "image/png" });

    // Comportamento atual: 413 sai como o erro generico com o status embutido — nao ha
    // tratamento distinto/UX dedicada para "arquivo grande demais" (ver issue).
    await expect(enviarMidia(arquivo)).rejects.toThrow(/413/);
  });

  it("lanca ErroDeMidia carregando o status 413 (a UI usa isso para a msg dedicada)", async () => {
    postMock.mockResolvedValue({
      data: undefined,
      error: { title: "Payload Too Large" },
      response: { status: 413 },
    });

    const arquivo = new File([new Uint8Array(10)], "grande.png", { type: "image/png" });

    const erro = await enviarMidia(arquivo).catch((e: unknown) => e);
    expect(erro).toBeInstanceOf(ErroDeMidia);
    expect((erro as ErroDeMidia).status).toBe(413);
  });
});

describe("removerMidia", () => {
  beforeEach(() => {
    deleteMock.mockReset();
  });

  it("chama DELETE /admin/midias/{id} com o id na rota", async () => {
    deleteMock.mockResolvedValue({ error: undefined, response: { status: 204 } });

    await removerMidia("11111111-1111-1111-1111-111111111111");

    expect(deleteMock).toHaveBeenCalledTimes(1);
    const [caminho, opcoes] = deleteMock.mock.calls[0] as [
      string,
      { params: { path: { id: string } } },
    ];
    expect(caminho).toBe("/admin/midias/{id}");
    expect(opcoes.params.path.id).toBe("11111111-1111-1111-1111-111111111111");
  });

  it("lanca erro quando a midia nao existe", async () => {
    deleteMock.mockResolvedValue({ error: {}, response: { status: 404 } });

    await expect(removerMidia("11111111-1111-1111-1111-111111111111")).rejects.toThrow(/404/);
  });
});
