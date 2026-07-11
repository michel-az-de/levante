import { beforeEach, describe, expect, it, vi } from "vitest";
import type { Artigo } from "@/types/domain";

vi.mock("@/lib/api", () => ({
  artigoApi: { GET: vi.fn() },
}));

import { generateMetadata } from "@/app/(site)/artigos/[slug]/page";
import { artigoApi } from "@/lib/api";

const artigoGet = vi.mocked(artigoApi.GET);

function artigo(slug: string, extras: Partial<Artigo> = {}): Artigo {
  return {
    id: "a1",
    titulo: "Titulo original",
    slug,
    resumo: "Resumo original.",
    conteudo: "...",
    status: "Publicado",
    dataPublicacao: "2026-01-15T12:00:00Z",
    metaTitulo: null,
    metaDescricao: null,
    imagemOgUrl: null,
    categoriaId: null,
    tags: [],
    ...extras,
  };
}

function respostaOk(data: Artigo) {
  return { data, response: { status: 200, ok: true } };
}

// Cada teste usa um slug proprio: obterArtigo usa React.cache() e memoiza por slug.
describe("generateMetadata de /artigos/[slug]", () => {
  beforeEach(() => {
    artigoGet.mockReset();
  });

  it("sem overrides usa titulo e resumo do artigo, com canonical do slug", async () => {
    const a = artigo("sem-overrides");
    artigoGet.mockResolvedValue(respostaOk(a) as never);

    const meta = await generateMetadata({ params: Promise.resolve({ slug: a.slug }) });

    expect(meta.title).toBe("Titulo original");
    expect(meta.description).toBe("Resumo original.");
    expect(meta.alternates?.canonical).toBe("/artigos/sem-overrides");
    expect(meta.openGraph?.images).toBeUndefined();
  });

  it("com meta SEO editavel os overrides vencem titulo/resumo e a imagem OG entra", async () => {
    const a = artigo("com-overrides", {
      metaTitulo: "Titulo para o Google",
      metaDescricao: "Descricao para a SERP.",
      imagemOgUrl: "https://cdn.exemplo/og.png",
    });
    artigoGet.mockResolvedValue(respostaOk(a) as never);

    const meta = await generateMetadata({ params: Promise.resolve({ slug: a.slug }) });

    expect(meta.title).toBe("Titulo para o Google");
    expect(meta.description).toBe("Descricao para a SERP.");
    expect(meta.openGraph?.title).toBe("Titulo para o Google");
    expect(meta.openGraph?.images).toEqual(["https://cdn.exemplo/og.png"]);
    expect(meta.twitter?.images).toEqual(["https://cdn.exemplo/og.png"]);
  });

  it("em 404 da API devolve metadata de nao encontrado (sem quebrar)", async () => {
    artigoGet.mockResolvedValue({ data: undefined, response: { status: 404, ok: false } } as never);

    const meta = await generateMetadata({ params: Promise.resolve({ slug: "nao-existe" }) });

    expect(meta.title).toBe("Artigo nao encontrado");
  });
});
