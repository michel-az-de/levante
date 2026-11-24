import { cleanup, render } from "@testing-library/react";
import { afterEach, describe, expect, it } from "vitest";
import { Timeline } from "@/components/site/Timeline";

describe("Timeline", () => {
  afterEach(cleanup);

  it("mostra a Avanade com o periodo encerrado", () => {
    const { container } = render(<Timeline />);
    const linha = [...container.querySelectorAll("div")].find(
      (div) => div.querySelector(":scope > span")?.textContent === "Avanade",
    );
    expect(linha).toBeDefined();
    expect(linha?.querySelector("[data-idioma-pt]")?.textContent).toBe("2025 a 2026");
    expect(linha?.querySelector("[data-idioma-en]")?.textContent).toBe("2025 to 2026");
  });
});
