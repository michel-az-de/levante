import { Idioma } from "@/components/Idioma";
import { fatos, pilhaTecnica } from "@/lib/site-conteudo";
import { Botao } from "./Botao";
import { CabecalhoSecao, Secao } from "./Secao";
import { Timeline } from "./Timeline";

/** Secao 05 — experiencia (sobre + fatos + linha do tempo). */
export function SecaoExperiencia() {
  return (
    <Secao id="experiencia">
      <CabecalhoSecao
        numero="05"
        kicker={{ pt: "experiência", en: "experience" }}
        titulo={{ pt: "Quinze anos de estrada.", en: "Fifteen years on the road." }}
        acao={
          <Botao href="/Felipe-Azevedo-CV.pdf" tamanho="sm" download>
            <Idioma pt="Baixar CV" en="Download CV" /> <span aria-hidden="true">↓</span>
          </Botao>
        }
      />

      <div className="mb-[46px] grid grid-cols-1 items-start gap-[50px] md:grid-cols-[1.1fr_0.9fr]">
        <div>
          <p className="mb-4 text-[17px] leading-[1.62] text-site-fg2">
            <Idioma
              pt={
                <>
                  Sou arquiteto de software e tech lead. Passei a maior parte desses anos em sistemas
                  distribuídos de missão crítica, em{" "}
                  <b className="font-semibold text-site-fg">bancos e financeiro</b>, onde errar custa
                  caro.
                </>
              }
              en={
                <>
                  I&apos;m a software architect and tech lead. I&apos;ve spent most of these years on
                  mission-critical distributed systems, in{" "}
                  <b className="font-semibold text-site-fg">banking and finance</b>, where mistakes
                  are expensive.
                </>
              }
            />
          </p>
          <p className="mb-4 text-[17px] leading-[1.62] text-site-fg2">
            <Idioma
              pt="Gosto de pegar o que está complicado e deixar simples: arquitetura que escala, é observável e fácil de manter. Lidero squads, tomo as decisões técnicas e construo minhas próprias ferramentas, de análise estática a engenharia com IA."
              en="I like taking what's complicated and making it simple: architecture that scales, stays observable and is easy to maintain. I lead squads, own the technical calls and build my own tools, from static analysis to AI-assisted engineering."
            />
          </p>
          <div className="mt-2 flex flex-wrap gap-2">
            {pilhaTecnica.map((item) => (
              <span
                key={item}
                className="border border-site-line px-2.5 py-[5px] font-site-mono text-xs text-site-fg2"
              >
                {item}
              </span>
            ))}
          </div>
        </div>

        <div className="flex flex-col border-t border-site-line">
          {fatos.map((fato) => (
            <div
              key={fato.chave.pt}
              className="flex justify-between gap-4 border-b border-site-line py-3.5 text-sm"
            >
              <span className="font-site-mono text-[11.5px] uppercase tracking-wider text-site-faint">
                <Idioma pt={fato.chave.pt} en={fato.chave.en} />
              </span>
              <span className="text-right text-site-fg">
                <Idioma pt={fato.valor.pt} en={fato.valor.en} />
              </span>
            </div>
          ))}
        </div>
      </div>

      <Timeline />
    </Secao>
  );
}
