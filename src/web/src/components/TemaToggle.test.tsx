import { cleanup, fireEvent, render, screen } from "@testing-library/react";
import { afterEach, describe, expect, it } from "vitest";
import { TemaToggle } from "@/components/TemaToggle";
import { IdiomaProvider } from "@/lib/i18n/IdiomaProvider";

describe("TemaToggle", () => {
  afterEach(() => {
    cleanup();
    localStorage.clear();
    document.documentElement.removeAttribute("data-theme");
  });

  it("alterna dark -> light -> dark e persiste em localStorage", () => {
    render(
      <IdiomaProvider>
        <TemaToggle />
      </IdiomaProvider>,
    );

    const botao = screen.getByRole("button", { name: "Alternar tema claro/escuro" });

    fireEvent.click(botao);
    expect(document.documentElement.getAttribute("data-theme")).toBe("light");
    expect(localStorage.getItem("levante:tema")).toBe("light");

    fireEvent.click(botao);
    expect(document.documentElement.getAttribute("data-theme")).toBe("dark");
    expect(localStorage.getItem("levante:tema")).toBe("dark");
  });
});
