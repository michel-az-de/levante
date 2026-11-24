import { cleanup, render } from "@testing-library/react";
import { afterEach, describe, expect, it } from "vitest";
import { SecaoExperiencia } from "@/components/site/SecaoExperiencia";

describe("SecaoExperiencia", () => {
  afterEach(cleanup);

  it("nao oferece download de CV enquanto o PDF nao existe", () => {
    const { container } = render(<SecaoExperiencia />);

    expect(container.querySelector('a[href$=".pdf"]')).toBeNull();
  });

  it("nao afirma um empregador atual nos fatos", () => {
    const { getAllByText, queryByText } = render(<SecaoExperiencia />);

    // A Avanade aparece so na linha do tempo, nao mais como fato "agora".
    expect(getAllByText("Avanade")).toHaveLength(1);
    expect(queryByText("agora")).toBeNull();
    expect(queryByText("now")).toBeNull();
  });

  it("aceita o numero da secao vindo da home", () => {
    const { container } = render(<SecaoExperiencia numero="04" />);

    expect(container.querySelector("#experiencia b")?.textContent).toBe("04");
  });
});
