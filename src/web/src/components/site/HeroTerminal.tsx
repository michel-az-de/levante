import { Idioma } from "@/components/Idioma";
import { terminalLinhas } from "@/lib/site-conteudo";
import { Botao } from "./Botao";

const PROMPT = "felipe@levante ~ %";

function TerminalHero() {
  return (
    <div className="border border-site-line2 bg-site-bg1/90 backdrop-blur-[4px]">
      <div className="flex items-center gap-[7px] border-b border-site-line px-3.5 py-[11px]">
        <i aria-hidden="true" className="h-[11px] w-[11px] rounded-full bg-[#ff5f57]" />
        <i aria-hidden="true" className="h-[11px] w-[11px] rounded-full bg-[#febc2e]" />
        <i aria-hidden="true" className="h-[11px] w-[11px] rounded-full bg-[#28c840]" />
        <span className="ml-2 font-site-mono text-[11.5px] text-site-faint">felipe@levante: ~</span>
      </div>
      <div className="min-h-[196px] p-[18px] font-site-mono text-[13.5px] leading-[2]">
        <div className="overflow-hidden text-ellipsis whitespace-nowrap">
          <span className="text-site-acc">{PROMPT}</span> <span className="text-site-fg">whoami</span>
        </div>
        {terminalLinhas.map((linha, i) => (
          <div
            key={linha.pt}
            className="site-term-linha overflow-hidden text-ellipsis whitespace-nowrap text-site-fg2"
            style={{ animationDelay: `${0.3 + i * 0.16}s` }}
          >
            <Idioma pt={linha.pt} en={linha.en} />
          </div>
        ))}
        <div className="whitespace-nowrap">
          <span className="text-site-acc">{PROMPT}</span> <span className="site-cursor" />
        </div>
      </div>
    </div>
  );
}

/** Hero da home: proposta a esquerda, terminal animado a direita. */
export function HeroTerminal() {
  return (
    <header className="py-[clamp(52px,9vw,116px)]">
      <div className="mx-auto grid max-w-[1180px] grid-cols-1 items-center gap-[clamp(28px,5vw,56px)] px-[clamp(18px,4vw,40px)] lg:grid-cols-[1.12fr_0.88fr]">
        <div>
          <div className="site-label mb-6 block">
            <Idioma
              pt="Arquiteto de software · Tech lead · .NET / Azure"
              en="Software architect · Tech lead · .NET / Azure"
            />
          </div>
          <h1 className="mb-6 max-w-[15ch] text-[clamp(38px,5.8vw,76px)] leading-[0.98] font-bold tracking-[-0.033em] text-site-fg">
            <Idioma
              pt={
                <>
                  Construo sistemas que <span className="text-site-acc">aguentam produção.</span>
                </>
              }
              en={
                <>
                  I build systems that <span className="text-site-acc">survive production.</span>
                </>
              }
            />
          </h1>
          <p className="mb-[30px] max-w-[46ch] text-[clamp(16px,1.8vw,19px)] leading-[1.55] text-site-fg2">
            <Idioma
              pt={
                <>
                  Quinze anos em <b className="font-medium text-site-fg">.NET e Azure</b>, no bancário
                  e financeiro. Hoje eu entrego com{" "}
                  <b className="font-medium text-site-fg">IA sob governança</b>: arquitetura e padrão
                  de time sênior, por uma fração do custo.
                </>
              }
              en={
                <>
                  Fifteen years in <b className="font-medium text-site-fg">.NET and Azure</b>, in
                  banking and finance. Today I deliver with{" "}
                  <b className="font-medium text-site-fg">AI under governance</b>: senior-team
                  architecture and standards, at a fraction of the cost.
                </>
              }
            />
          </p>
          <div className="flex flex-wrap items-center gap-3">
            <Botao href="#contato" variante="acc" magnetico>
              <Idioma pt="Falar comigo" en="Get in touch" />
            </Botao>
            <Botao href="#consultoria">
              <Idioma pt="Como eu entrego" en="How I deliver" />
            </Botao>
            <span className="site-label ml-1.5 flex items-center gap-2">
              <span aria-hidden="true" className="h-[7px] w-[7px] rounded-full bg-site-acc" />
              <Idioma
                pt="São Paulo · consultoria, projetos e freelas"
                en="São Paulo · consulting, projects and freelance"
              />
            </span>
          </div>
        </div>
        <TerminalHero />
      </div>
    </header>
  );
}
