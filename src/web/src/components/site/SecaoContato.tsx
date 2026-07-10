import { Idioma } from "@/components/Idioma";
import { Botao } from "./Botao";

const EMAIL = "felipe.azevedoit@gmail.com";
const WHATSAPP = "https://wa.me/5511982254398";
const LINKEDIN = "https://linkedin.com/in/felipe-azevedo-05493357";
const GITHUB = "https://github.com/felipeazevedoit";

const linkEmail =
  "border-b-[3px] border-transparent text-site-acc transition-colors hover:border-site-acc";

/** Secao 06 — contato. */
export function SecaoContato() {
  return (
    <section id="contato" className="border-t border-site-line py-[clamp(70px,11vw,132px)]">
      <div className="mx-auto max-w-[1180px] px-[clamp(18px,4vw,40px)]">
        <div className="site-label mb-[22px]">
          <b className="font-medium text-site-acc">06</b> / <Idioma pt="contato" en="contact" />
        </div>
        <h2 className="mb-6 max-w-[14ch] text-[clamp(40px,8vw,92px)] leading-[0.96] font-bold tracking-[-0.035em] text-site-fg">
          <Idioma
            pt={
              <>
                Vamos{" "}
                <a href={`mailto:${EMAIL}`} className={linkEmail}>
                  conversar
                </a>
                .
              </>
            }
            en={
              <>
                Let&apos;s{" "}
                <a href={`mailto:${EMAIL}`} className={linkEmail}>
                  talk
                </a>
                .
              </>
            }
          />
        </h2>
        <p className="mb-[30px] max-w-[46ch] text-lg text-site-fg2">
          <Idioma
            pt="Consultoria, projeto fechado, alocação ou freela. Me conta o problema, eu digo como entrego."
            en="Consulting, fixed project, staffing or freelance. Tell me the problem, I'll tell you how I deliver."
          />
        </p>
        <div className="flex flex-wrap gap-3">
          <Botao href={`mailto:${EMAIL}`} variante="acc" magnetico>
            <Idioma pt="Mandar e-mail" en="Send email" />
          </Botao>
          <Botao href={WHATSAPP} target="_blank" rel="noopener noreferrer">
            WhatsApp
          </Botao>
        </div>
        <div className="mt-[34px] flex flex-wrap gap-[26px] border-t border-site-line pt-6 font-site-mono text-[13px]">
          <a href={`mailto:${EMAIL}`} className="text-site-fg2 transition-colors hover:text-site-acc">
            {EMAIL}
          </a>
          <a
            href={LINKEDIN}
            target="_blank"
            rel="noopener noreferrer"
            className="text-site-fg2 transition-colors hover:text-site-acc"
          >
            linkedin ↗
          </a>
          <a
            href={GITHUB}
            target="_blank"
            rel="noopener noreferrer"
            className="text-site-fg2 transition-colors hover:text-site-acc"
          >
            github ↗
          </a>
        </div>
      </div>
    </section>
  );
}
