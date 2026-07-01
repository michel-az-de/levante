import { cleanup, render, screen } from "@testing-library/react";
import { afterEach, describe, expect, it } from "vitest";
import { Markdown } from "@/components/Markdown";

describe("Markdown (render seguro)", () => {
  afterEach(cleanup);

  it("renderiza markdown basico como elementos React", () => {
    render(<Markdown>{"# Titulo\n\nUm **paragrafo**."}</Markdown>);

    expect(screen.getByRole("heading", { level: 1 }).textContent).toBe("Titulo");
    expect(screen.getByText("paragrafo").tagName).toBe("STRONG");
  });

  it("renderiza tabela GFM (remark-gfm ativo)", () => {
    render(<Markdown>{"| a | b |\n| - | - |\n| 1 | 2 |"}</Markdown>);

    expect(screen.getByRole("table")).toBeTruthy();
  });

  it("NAO renderiza HTML cru: script vira texto, nao elemento (anti-XSS)", () => {
    const { container } = render(
      <Markdown>{'<script>window.hackeado = true;</script><img src=x onerror="window.hackeado=true">'}</Markdown>,
    );

    expect(container.querySelector("script")).toBeNull();
    expect(container.querySelector("img")).toBeNull();
    expect((window as unknown as { hackeado?: boolean }).hackeado).toBeUndefined();
  });
});
