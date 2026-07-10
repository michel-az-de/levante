import { cleanup, fireEvent, render, screen } from "@testing-library/react";
import { afterEach, describe, expect, it } from "vitest";
import { IdiomaToggle } from "@/components/IdiomaToggle";
import { IdiomaProvider } from "@/lib/i18n/IdiomaProvider";

describe("IdiomaToggle", () => {
  afterEach(() => {
    cleanup();
    localStorage.clear();
    document.documentElement.removeAttribute("data-idioma");
  });

  it("alterna o idioma ao clicar", () => {
    render(
      <IdiomaProvider>
        <IdiomaToggle />
      </IdiomaProvider>,
    );

    fireEvent.click(screen.getByRole("button", { name: "Mudar para inglês" }));

    expect(document.documentElement.getAttribute("data-idioma")).toBe("en");
  });
});
