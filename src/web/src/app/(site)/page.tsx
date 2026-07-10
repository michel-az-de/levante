import type { Metadata } from "next";
import { JsonLd } from "@/components/JsonLd";
import { HeroTerminal } from "@/components/site/HeroTerminal";
import { SecaoArtigos } from "@/components/site/SecaoArtigos";
import { SecaoCapacidades } from "@/components/site/SecaoCapacidades";
import { SecaoCodigoAberto } from "@/components/site/SecaoCodigoAberto";
import { SecaoConsultoria } from "@/components/site/SecaoConsultoria";
import { SecaoContato } from "@/components/site/SecaoContato";
import { SecaoExperiencia } from "@/components/site/SecaoExperiencia";
import { site } from "@/lib/site";

// A secao de artigos precisa estar fresca (novos artigos aparecem); os dados do
// GitHub tem cache proprio por fetch (revalidate), entao continuam cacheados
// mesmo com a rota dinamica.
export const dynamic = "force-dynamic";

export const metadata: Metadata = {
  alternates: { canonical: "/" },
  openGraph: { title: site.nome, description: site.descricao, url: site.url },
};

const pessoa = {
  "@context": "https://schema.org",
  "@type": "Person",
  name: site.autor,
  jobTitle: site.jobTitle,
  url: site.url,
  sameAs: site.sameAs,
  knowsAbout: site.knowsAbout,
};

const website = {
  "@context": "https://schema.org",
  "@type": "WebSite",
  name: site.nome,
  url: site.url,
};

export default function HomePage() {
  return (
    <>
      <JsonLd data={pessoa} />
      <JsonLd data={website} />
      <HeroTerminal />
      <SecaoConsultoria />
      <SecaoCapacidades />
      <SecaoCodigoAberto />
      <SecaoArtigos />
      <SecaoExperiencia />
      <SecaoContato />
    </>
  );
}
