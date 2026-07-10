import type { Metadata } from "next";
import { AutorCard } from "@/components/AutorCard";
import { site } from "@/lib/site";
import { Arquitetura } from "./_components/Arquitetura";
import { CtaFinal } from "./_components/CtaFinal";
import { Headless } from "./_components/Headless";
import { Hero } from "./_components/Hero";
import { Recursos } from "./_components/Recursos";
import { Repo } from "./_components/Repo";
import { CabecalhoProduto, SecaoProduto } from "./_components/SecaoProduto";

const DESCRICAO =
  "Levante é uma engine de publicação headless em .NET. API-first, núcleo orientado a eventos, self-hostable.";

// force-dynamic: renderiza fresco a cada request; os dados do GitHub tem cache
// proprio por fetch (revalidate), entao continuam cacheados.
export const dynamic = "force-dynamic";

export const metadata: Metadata = {
  title: { absolute: "Levante — engine de publicação open source" },
  description: DESCRICAO,
  alternates: { canonical: "/levante" },
  openGraph: {
    title: "Levante — engine de publicação open source",
    description: DESCRICAO,
    url: `${site.url}/levante`,
  },
};

export default function LevantePage() {
  return (
    <>
      <Hero />
      <Recursos />
      <Headless />
      <Arquitetura />
      <Repo />
      <SecaoProduto id="autor">
        <CabecalhoProduto
          kicker={{ pt: "O autor", en: "The author" }}
          titulo={{ pt: "Quem mantém o Levante.", en: "Who maintains Levante." }}
        />
        <AutorCard
          mailtoHref="mailto:felipe.azevedoit@gmail.com"
          githubUrl="https://github.com/felipeazevedoit"
        />
      </SecaoProduto>
      <CtaFinal />
    </>
  );
}
