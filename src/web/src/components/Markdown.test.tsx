import { cleanup, render, screen } from "@testing-library/react";
import { afterEach, describe, expect, it } from "vitest";
import { Markdown } from "@/components/Markdown";
import { extrairTitulos } from "@/lib/artigos";

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

  it("id da heading com link bate com o id do TOC (extrairTitulos)", () => {
    const md = "## Veja [o guia](https://exemplo.com)";
    const { container } = render(<Markdown>{md}</Markdown>);
    const [titulo] = extrairTitulos(md);

    // A âncora do TOC (extrairTitulos) e o id renderizado precisam coincidir.
    expect(container.querySelector("h2")?.id).toBe(titulo.id);
    expect(titulo.id).toBe("veja-o-guia");
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
