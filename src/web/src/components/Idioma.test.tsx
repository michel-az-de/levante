import { cleanup, render, screen } from "@testing-library/react";
import { afterEach, describe, expect, it } from "vitest";
import { Idioma } from "@/components/Idioma";

describe("Idioma", () => {
  afterEach(cleanup);

  it("renderiza os dois idiomas com lang e marcador corretos", () => {
    render(<Idioma pt="Artigos" en="Writing" />);

    const pt = screen.getByText("Artigos");
    const en = screen.getByText("Writing");

    expect(pt.getAttribute("lang")).toBe("pt-BR");
    expect(pt.hasAttribute("data-idioma-pt")).toBe(true);
    expect(en.getAttribute("lang")).toBe("en");
    expect(en.hasAttribute("data-idioma-en")).toBe(true);
  });
});
