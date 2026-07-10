import { Idioma } from "@/components/Idioma";
import { metodos } from "@/lib/site-conteudo";
import { Botao } from "./Botao";
import { CardMetodo } from "./CardMetodo";
import { DiagramaLoopEntrega } from "./DiagramaLoopEntrega";
import { Marca } from "./Marca";
import { RevealOnScroll } from "./RevealOnScroll";
import { CabecalhoSecao, Secao } from "./Secao";

const formatos: readonly { pt: string; en: string }[] = [
  { pt: "projeto fechado", en: "fixed project" },
  { pt: "alocação", en: "staffing" },
  { pt: "freela", en: "freela" },
  { pt: "consultoria de adoção", en: "adoption consulting" },
];

/** Secao 01 — consultoria: metodo de entrega com IA sob governanca. */
export function SecaoConsultoria() {
  return (
    <Secao id="consultoria">
      <CabecalhoSecao
        numero="01"
        kicker={{
          pt: "consultoria · gestão de projetos com IA",
          en: "consulting · AI delivery",
        }}
        titulo={{ pt: "Time sênior, velocidade de IA.", en: "Senior team, AI speed." }}
        subtitulo={{
          pt: (
            <>
              Eu uso IA para construir <b className="font-medium text-site-fg">aplicações reais</b>,
              com arquitetura e padrões de verdade, por uma fração do custo de um time tradicional.
              Não é protótipo descartável, é software que vai pra produção.
            </>
          ),
          en: (
            <>
              I use AI to build <b className="font-medium text-site-fg">real applications</b>, with
              real architecture and patterns, at a fraction of a traditional team&apos;s cost. Not a
              throwaway prototype, software that ships to production.
            </>
          ),
        }}
        acao={
          <Botao href="#contato" tamanho="sm">
            <Idioma pt="Quero conversar" en="Let's talk" /> <span aria-hidden="true">→</span>
          </Botao>
        }
      />

      <RevealOnScroll className="grid grid-cols-1 gap-px border border-site-line bg-site-line md:grid-cols-2">
        {metodos.map((metodo) => (
          <CardMetodo key={metodo.numero} metodo={metodo} />
        ))}
      </RevealOnScroll>

      <RevealOnScroll className="mt-6 border border-site-line2 bg-site-bg1">
        <div className="site-label flex items-center gap-2.5 border-b border-site-line px-4 py-3">
          <span className="site-pill">FIG. 01</span>
          <Idioma pt="o loop de entrega" en="the delivery loop" />
        </div>
        <div className="overflow-x-auto p-[clamp(20px,4vw,34px)]">
          <DiagramaLoopEntrega />
        </div>
        <p className="border-t border-site-line px-4 py-3 text-[13.5px] text-site-fg2">
          <Idioma
            pt="A IA gera e executa. O arquiteto define, revisa e prova. Se a revisão acha gap ou a prova reprova, o trabalho volta. Antes de chegar em você."
            en="AI generates and executes. The architect defines, reviews and proves. If review finds a gap or the proof fails, the work goes back. Before it reaches you."
          />
        </p>
      </RevealOnScroll>

      <div className="mt-[22px] flex flex-wrap items-center justify-between gap-[18px]">
        <span className="flex items-center gap-2 font-site-mono text-[12.5px] text-site-fg2">
          <Marca className="h-3.5 w-3.5" />
          <Idioma
            pt="é assim que eu construo o EasyStock e o Levante"
            en="this is how I build EasyStock and Levante"
          />
        </span>
        <div className="flex flex-wrap gap-2">
          {formatos.map((formato) => (
            <span
              key={formato.pt}
              className="border border-site-line2 px-2.5 py-[5px] font-site-mono text-[11px] text-site-faint transition-colors hover:border-site-acc hover:text-site-acc"
            >
              <Idioma pt={formato.pt} en={formato.en} />
            </span>
          ))}
        </div>
      </div>
    </Secao>
  );
}
