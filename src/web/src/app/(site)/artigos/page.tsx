import type { Metadata } from "next";
import { Idioma } from "@/components/Idioma";
import { ListaArtigos } from "@/components/site/artigo/ListaArtigos";
import { artigoApi } from "@/lib/api";
import type { Artigo } from "@/types/domain";

// SSR a cada request (lista sempre fresca; nao exige a API no next build do CI).
export const dynamic = "force-dynamic";

export const metadata: Metadata = {
  title: "Artigos",
  description: "Artigos tecnicos publicados no Levante.",
  alternates: { canonical: "/artigos" },
  openGraph: { title: "Artigos", url: "/artigos" },
};

export default async function ArtigosPage() {
  let artigos: Artigo[] = [];
  let erro = false;
  try {
    const { data, error } = await artigoApi.GET("/artigos");
    if (error) {
      erro = true;
    } else {
      artigos = data ?? [];
    }
  } catch {
    erro = true;
  }

  return (
    <div className="mx-auto max-w-[1180px] px-[clamp(18px,4vw,40px)] py-[clamp(48px,8vw,92px)]">
      <div className="site-label mb-3.5">
        <b className="font-medium text-site-acc">
          <Idioma pt="escrita" en="writing" />
        </b>{" "}
        · Levante
      </div>
      <h1 className="mb-4 text-[clamp(36px,6vw,66px)] leading-none font-bold tracking-[-0.03em] text-site-fg">
        <Idioma pt="Artigos" en="Articles" />
      </h1>
      <p className="mb-8 max-w-[54ch] text-base text-site-fg2">
        <Idioma
          pt="Tudo o que escrevo sobre arquitetura, .NET, mensageria e sistemas distribuídos."
          en="Everything I write about architecture, .NET, messaging and distributed systems."
        />
      </p>
      {erro ? (
        <p className="text-red-500">
          <Idioma pt="Não foi possível carregar os artigos." en="Could not load the articles." />
        </p>
      ) : (
        <ListaArtigos artigos={artigos} />
      )}
    </div>
  );
}
