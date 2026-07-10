import { Idioma } from "@/components/Idioma";
import { experiencias } from "@/lib/site-conteudo";

/** Linha do tempo de experiencia profissional (secao Experiencia). */
export function Timeline() {
  return (
    <div className="border-t border-site-line">
      {experiencias.map((xp) => (
        <div
          key={xp.empresa}
          className="grid grid-cols-[1fr_auto] items-baseline gap-x-6 gap-y-1.5 border-b border-site-line py-6"
        >
          <span className="text-2xl font-bold tracking-tight text-site-fg">{xp.empresa}</span>
          <span className="font-site-mono text-xs whitespace-nowrap text-site-faint">
            <Idioma pt={xp.periodo.pt} en={xp.periodo.en} />
          </span>
          <span className="col-start-1 text-sm text-site-fg2">
            <Idioma pt={xp.papel.pt} en={xp.papel.en} />
          </span>
        </div>
      ))}
    </div>
  );
}
