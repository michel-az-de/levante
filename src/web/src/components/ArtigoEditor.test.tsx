import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import { cleanup, fireEvent, render, screen, waitFor } from "@testing-library/react";
import { ArtigoEditor, type ArtigoFormValores } from "@/components/ArtigoEditor";
import { ErroDeMidia } from "@/lib/midias";

// Categorias carregam via apiAdmin.GET no mount; mockar evita fetch real (URL relativa fora do browser).
vi.mock("@/lib/auth", () => ({
  apiAdmin: { GET: vi.fn().mockResolvedValue({ data: [] }) },
}));

// O preview usa <Markdown> (react-markdown); passthrough mantem o teste focado no editor.
vi.mock("@/components/Markdown", () => ({
  Markdown: ({ children }: { children: string }) => children,
}));

// Upload de imagem: enviarMidia controlavel + ErroDeMidia real (para o instanceof do componente).
const { enviarMidiaMock } = vi.hoisted(() => ({ enviarMidiaMock: vi.fn() }));
vi.mock("@/lib/midias", () => ({
  enviarMidia: enviarMidiaMock,
  ErroDeMidia: class ErroDeMidia extends Error {
    constructor(
      mensagem: string,
      readonly status: number,
    ) {
      super(mensagem);
      this.name = "ErroDeMidia";
    }
  },
}));

const VAZIO: ArtigoFormValores = {
  titulo: "",
  slug: "",
  resumo: "",
  conteudo: "",
  metaTitulo: "",
  metaDescricao: "",
  imagemOgUrl: "",
  categoriaId: "",
  tags: [],
};

function renderizar(
  onSubmit: (v: ArtigoFormValores) => Promise<string | null>,
  inicial: Partial<ArtigoFormValores> = {},
) {
  return render(<ArtigoEditor inicial={{ ...VAZIO, ...inicial }} textoAcao="Criar" onSubmit={onSubmit} />);
}

describe("ArtigoEditor", () => {
  afterEach(cleanup);
  beforeEach(() => enviarMidiaMock.mockReset());

  it("mostra os chips de tag normalizados (kebab-case, sem vazias nem duplicadas)", () => {
    renderizar(vi.fn().mockResolvedValue(null));
    const tags = screen.getByPlaceholderText("clean-architecture, ddd, dotnet");

    fireEvent.change(tags, { target: { value: "Clean Architecture, DDD, ddd, ," } });

    expect(screen.getByText("clean-architecture")).toBeTruthy();
    expect(screen.getAllByText("ddd")).toHaveLength(1); // dedup; ", ," nao vira chip vazio
  });

  it("a toolbar 'Negrito' envolve a selecao (ou o exemplo) com **", () => {
    renderizar(vi.fn().mockResolvedValue(null));

    fireEvent.click(screen.getByRole("button", { name: "Negrito" }));

    expect(screen.getByDisplayValue("**negrito**")).toBeTruthy();
  });

  it("submete com os valores preenchidos e as tags ja parseadas", async () => {
    const onSubmit = vi.fn().mockResolvedValue(null);
    const { container } = renderizar(onSubmit);

    fireEvent.change(screen.getByLabelText("Titulo"), { target: { value: "Meu Artigo" } });
    fireEvent.change(screen.getByPlaceholderText("clean-architecture-na-pratica"), { target: { value: "meu-artigo" } });
    fireEvent.change(screen.getByLabelText(/Resumo/), { target: { value: "Resumo." } });
    fireEvent.change(container.querySelector("textarea[rows='18']")!, { target: { value: "# Conteudo" } });
    fireEvent.change(screen.getByPlaceholderText("clean-architecture, ddd, dotnet"), {
      target: { value: "Clean Architecture, DDD, ddd" },
    });

    fireEvent.click(screen.getByRole("button", { name: "Criar" }));

    await waitFor(() => expect(onSubmit).toHaveBeenCalledTimes(1));
    expect(onSubmit).toHaveBeenCalledWith(
      expect.objectContaining({
        titulo: "Meu Artigo",
        slug: "meu-artigo",
        resumo: "Resumo.",
        conteudo: "# Conteudo",
        tags: ["clean-architecture", "ddd"],
      }),
    );
  });

  it("exibe a mensagem de erro devolvida pelo onSubmit", async () => {
    const { container } = renderizar(vi.fn().mockResolvedValue("Slug ja existe."));

    // submit direto (nao clique): jsdom bloquearia o clique com os campos required vazios,
    // e aqui o alvo e o handler de submit, nao a validacao HTML5.
    fireEvent.submit(container.querySelector("form")!);

    expect(await screen.findByText("Slug ja existe.")).toBeTruthy();
  });

  it("exibe 'Falha de conexao' quando o onSubmit rejeita (rede)", async () => {
    const { container } = renderizar(vi.fn().mockRejectedValue(new Error("network")));

    fireEvent.submit(container.querySelector("form")!);

    expect(await screen.findByText(/Falha de conexao/)).toBeTruthy();
  });

  it("desabilita o botao enquanto envia e reabilita ao terminar", async () => {
    let resolver: (v: string | null) => void = () => {};
    const onSubmit = vi.fn(
      () =>
        new Promise<string | null>((r) => {
          resolver = r;
        }),
    );
    const { container } = renderizar(onSubmit);

    const botao = screen.getByRole("button", { name: "Criar" }) as HTMLButtonElement;
    fireEvent.submit(container.querySelector("form")!);

    await waitFor(() => expect(botao.disabled).toBe(true));
    expect(botao.textContent).toBe("Salvando...");

    resolver(null);
    await waitFor(() => expect(botao.disabled).toBe(false));
  });

  it("faz upload de imagem valida e insere ![alt](url) no conteudo", async () => {
    enviarMidiaMock.mockResolvedValue({ id: "abc", url: "/midias/abc", contentType: "image/png", tamanho: 3 });
    const { container } = renderizar(vi.fn().mockResolvedValue(null));
    const input = container.querySelector<HTMLInputElement>("input[type='file']")!;

    fireEvent.change(input, {
      target: { files: [new File([new Uint8Array([1, 2, 3])], "foto.png", { type: "image/png" })] },
    });

    expect(await screen.findByDisplayValue("![foto](/midias/abc)")).toBeTruthy();
    expect(enviarMidiaMock).toHaveBeenCalledTimes(1);
  });

  it("rejeita tipo nao suportado sem chamar a API", async () => {
    const { container } = renderizar(vi.fn().mockResolvedValue(null));
    const input = container.querySelector<HTMLInputElement>("input[type='file']")!;

    fireEvent.change(input, { target: { files: [new File(["x"], "doc.pdf", { type: "application/pdf" })] } });

    expect(await screen.findByText(/Tipo nao suportado/)).toBeTruthy();
    expect(enviarMidiaMock).not.toHaveBeenCalled();
  });

  it("rejeita imagem acima de 5 MB sem chamar a API", async () => {
    const { container } = renderizar(vi.fn().mockResolvedValue(null));
    const input = container.querySelector<HTMLInputElement>("input[type='file']")!;
    const grande = new File(["x"], "grande.png", { type: "image/png" });
    Object.defineProperty(grande, "size", { value: 6 * 1024 * 1024 });

    fireEvent.change(input, { target: { files: [grande] } });

    expect(await screen.findByText("Imagem maior que o limite de 5 MB.")).toBeTruthy();
    expect(enviarMidiaMock).not.toHaveBeenCalled();
  });

  it("mostra mensagem dedicada quando a API responde 413", async () => {
    enviarMidiaMock.mockRejectedValue(new ErroDeMidia("Falha ao enviar midia (HTTP 413).", 413));
    const { container } = renderizar(vi.fn().mockResolvedValue(null));
    const input = container.querySelector<HTMLInputElement>("input[type='file']")!;

    fireEvent.change(input, {
      target: { files: [new File([new Uint8Array([1])], "foto.png", { type: "image/png" })] },
    });

    expect(await screen.findByText("Imagem maior que o limite de 5 MB.")).toBeTruthy();
  });
});
