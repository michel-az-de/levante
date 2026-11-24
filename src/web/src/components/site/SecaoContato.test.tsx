import { cleanup, render } from "@testing-library/react";
import { afterEach, beforeAll, describe, expect, it } from "vitest";
import { SecaoContato } from "@/components/site/SecaoContato";
import { site } from "@/lib/site";

describe("SecaoContato", () => {
  // O botao magnetico consulta matchMedia, que o jsdom nao implementa.
  beforeAll(() => {
    window.matchMedia = ((query: string) => ({ matches: false, media: query })) as never;
  });
  afterEach(cleanup);

  it("linka o LinkedIn configurado no site", () => {
    const { container } = render(<SecaoContato />);

    const hrefs = Array.from(container.querySelectorAll("a")).map((a) => a.getAttribute("href"));
    expect(hrefs).toContain(site.linkedin);
    const hostsLinkedin = new Set(["linkedin.com", "www.linkedin.com"]);
    const doLinkedin = hrefs.filter((h) => h && hostsLinkedin.has(new URL(h, site.url).hostname));
    expect(doLinkedin).toEqual([site.linkedin]);
  });

  it("aceita o numero da secao vindo da home", () => {
    const { container } = render(<SecaoContato numero="05" />);

    expect(container.querySelector("#contato b")?.textContent).toBe("05");
  });
});
