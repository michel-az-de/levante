import { cleanup, fireEvent, render, screen } from "@testing-library/react";
import { afterEach, describe, expect, it, vi } from "vitest";
import { IdiomaProvider, useIdioma } from "@/lib/i18n/IdiomaProvider";

function Sonda() {
  const { idioma, alternar, t } = useIdioma();
  return (
    <div>
      <span data-testid="idioma">{idioma}</span>
      <span data-testid="rotulo">{t("alternarTema")}</span>
      <button type="button" onClick={alternar}>
        trocar
      </button>
    </div>
  );
}

describe("IdiomaProvider / useIdioma", () => {
  afterEach(() => {
    cleanup();
    localStorage.clear();
    document.documentElement.removeAttribute("data-idioma");
  });

  it("comeca em pt e traduz pela chave", () => {
    render(
      <IdiomaProvider>
        <Sonda />
      </IdiomaProvider>,
    );

    expect(screen.getByTestId("idioma").textContent).toBe("pt");
    expect(screen.getByTestId("rotulo").textContent).toBe("Alternar tema claro/escuro");
  });

  it("alterna para en, ajusta data-idioma no html e persiste em localStorage", () => {
    render(
      <IdiomaProvider>
        <Sonda />
      </IdiomaProvider>,
    );

    fireEvent.click(screen.getByText("trocar"));

    expect(screen.getByTestId("idioma").textContent).toBe("en");
    expect(document.documentElement.getAttribute("data-idioma")).toBe("en");
    expect(localStorage.getItem("levante:idioma")).toBe("en");
    expect(screen.getByTestId("rotulo").textContent).toBe("Toggle light/dark theme");
  });

  it("sincroniza com data-idioma pre-ajustado pelo script anti-FOUC", () => {
    document.documentElement.setAttribute("data-idioma", "en");

    render(
      <IdiomaProvider>
        <Sonda />
      </IdiomaProvider>,
    );

    expect(screen.getByTestId("idioma").textContent).toBe("en");
  });

  it("useIdioma sem provider lanca erro", () => {
    const spy = vi.spyOn(console, "error").mockImplementation(() => {});
    expect(() => render(<Sonda />)).toThrow(/IdiomaProvider/);
    spy.mockRestore();
  });
});
