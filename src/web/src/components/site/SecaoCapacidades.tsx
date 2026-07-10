import { capacidades } from "@/lib/site-conteudo";
import { CardCapacidade } from "./CardCapacidade";
import { RevealOnScroll } from "./RevealOnScroll";
import { CabecalhoSecao, Secao } from "./Secao";
import { SnippetOutbox } from "./SnippetOutbox";

/** Secao 02 — o que eu entrego (capacidades + snippet). */
export function SecaoCapacidades() {
  return (
    <Secao id="capacidades">
      <CabecalhoSecao
        numero="02"
        kicker={{ pt: "o que eu faço", en: "what I do" }}
        titulo={{ pt: "O que eu entrego.", en: "What I deliver." }}
        subtitulo={{
          pt: "Para squads, projetos fechados e freelas. O padrão é o mesmo em todos.",
          en: "For squads, fixed projects and freelance. The standard is the same in all of them.",
        }}
      />

      <RevealOnScroll className="mb-6 grid grid-cols-1 gap-px border border-site-line bg-site-line md:grid-cols-2">
        {capacidades.map((capacidade) => (
          <CardCapacidade key={capacidade.numero} capacidade={capacidade} />
        ))}
      </RevealOnScroll>

      <RevealOnScroll>
        <SnippetOutbox />
      </RevealOnScroll>
    </Secao>
  );
}
