import { cleanup, fireEvent, render, screen } from "@testing-library/react";
import { afterEach, describe, expect, it } from "vitest";
import { MenuMobile, type ItemMenuMobile } from "@/components/MenuMobile";
import { IdiomaProvider } from "@/lib/i18n/IdiomaProvider";

const ITENS: readonly ItemMenuMobile[] = [
  { href: "#recursos", pt: "Recursos", en: "Features" },
  { href: "/artigos", pt: "Artigos", en: "Writing" },
  { href: "https://github.com/michel-az-de/levante", pt: "GitHub", en: "GitHub", externo: true },
];

function renderizar() {
  return render(
    <IdiomaProvider>
      <MenuMobile
        id="menu-teste"
        itens={ITENS}
        classeContainer=""
        classeBotao=""
        classePainel=""
        classeItem=""
      />
    </IdiomaProvider>,
  );
}

describe("MenuMobile", () => {
  afterEach(() => {
    cleanup();
  });

  it("comeca fechado e abre com todos os itens ao clicar no toggle", () => {
    renderizar();

    const toggle = screen.getByRole("button", { name: /abrir menu/i });
    expect(toggle.getAttribute("aria-expanded")).toBe("false");
    expect(screen.queryByText("Recursos")).toBeNull();

    fireEvent.click(toggle);

    expect(screen.getByRole("button", { name: /fechar menu/i }).getAttribute("aria-expanded")).toBe(
      "true",
    );
    expect(screen.getByText("Recursos")).toBeTruthy();
    expect(screen.getByText("Artigos")).toBeTruthy();

    const externo = screen.getByText("GitHub").closest("a");
    expect(externo?.getAttribute("target")).toBe("_blank");
    expect(externo?.getAttribute("rel")).toContain("noopener");
  });

  it("fecha o painel ao escolher um item", () => {
    renderizar();

    fireEvent.click(screen.getByRole("button", { name: /abrir menu/i }));
    fireEvent.click(screen.getByText("Recursos"));

    expect(screen.queryByText("Recursos")).toBeNull();
    expect(
      screen.getByRole("button", { name: /abrir menu/i }).getAttribute("aria-expanded"),
    ).toBe("false");
  });
});
