import { cleanup, fireEvent, render, screen } from "@testing-library/react";
import { afterEach, describe, expect, it, vi } from "vitest";
import { CodeTabs } from "@/app/(produto)/levante/_components/CodeTabs";
import { IdiomaProvider } from "@/lib/i18n/IdiomaProvider";

const abas = [
  { id: "docker", rotulo: "docker", comando: "docker run x" },
  { id: "source", rotulo: "source", comando: "git clone y" },
];

function montar() {
  return render(
    <IdiomaProvider>
      <CodeTabs abas={abas} />
    </IdiomaProvider>,
  );
}

describe("CodeTabs", () => {
  afterEach(() => {
    cleanup();
    localStorage.clear();
    document.documentElement.removeAttribute("data-idioma");
  });

  it("mostra a primeira aba e troca ao clicar", () => {
    montar();
    expect(screen.getByText("docker run x")).toBeTruthy();

    fireEvent.click(screen.getByRole("tab", { name: "source" }));
    expect(screen.getByText("git clone y")).toBeTruthy();
  });

  it("copia o comando ativo e mostra o feedback", async () => {
    const writeText = vi.fn().mockResolvedValue(undefined);
    Object.defineProperty(navigator, "clipboard", { value: { writeText }, configurable: true });

    montar();
    fireEvent.click(screen.getByRole("button", { name: "copiar" }));

    await screen.findByText("copiado");
    expect(writeText).toHaveBeenCalledWith("docker run x");
  });
});
