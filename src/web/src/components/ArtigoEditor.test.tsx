import { afterEach, describe, expect, it, vi } from "vitest";
import { cleanup, fireEvent, render, screen, waitFor } from "@testing-library/react";
import { ArtigoEditor, type ArtigoFormValores } from "@/components/ArtigoEditor";

// Categorias carregam via apiAdmin.GET no mount; mockar evita fetch real (URL relativa fora do browser).
vi.mock("@/lib/auth", () => ({
  apiAdmin: { GET: vi.fn().mockResolvedValue({ data: [] }) },
}));

// O preview usa <Markdown> (react-markdown); passthrough mantem o teste focado no editor.
vi.mock("@/components/Markdown", () => ({
  Markdown: ({ children }: { children: string }) => children,
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
    renderizar(vi.fn().mockResolvedValue("Slug ja existe."));

    fireEvent.click(screen.getByRole("button", { name: "Criar" }));

    expect(await screen.findByText("Slug ja existe.")).toBeTruthy();
  });

  it("exibe 'Falha de conexao' quando o onSubmit rejeita (rede)", async () => {
    renderizar(vi.fn().mockRejectedValue(new Error("network")));

    fireEvent.click(screen.getByRole("button", { name: "Criar" }));

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
    renderizar(onSubmit);

    const botao = screen.getByRole("button", { name: "Criar" }) as HTMLButtonElement;
    fireEvent.click(botao);

    await waitFor(() => expect(botao.disabled).toBe(true));
    expect(botao.textContent).toBe("Salvando...");

    resolver(null);
    await waitFor(() => expect(botao.disabled).toBe(false));
  });
});
