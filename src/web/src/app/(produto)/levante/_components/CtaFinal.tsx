import { Idioma } from "@/components/Idioma";
import { produto } from "@/lib/produto";
import { BotaoProduto } from "./BotaoProduto";
import { SecaoProduto } from "./SecaoProduto";

/** Secao final: chamada open source + metadados. */
export function CtaFinal() {
  return (
    <SecaoProduto id="docs">
      <div className="mx-auto max-w-[820px] overflow-hidden rounded-2xl border border-produto-line2 bg-produto-bg1 p-[clamp(28px,5vw,48px)] text-center">
        <span
          aria-hidden="true"
          className="mb-[18px] inline-block h-6 w-6 rotate-45 border-2 border-produto-brass"
        />
        <h2 className="mb-3 text-[clamp(24px,3.6vw,32px)] font-bold text-produto-fg">
          <Idioma pt="Open source. Self-hostable. Seu." en="Open source. Self-hostable. Yours." />
        </h2>
        <p className="mx-auto mb-6 max-w-[52ch] text-[15.5px] text-produto-dim">
          <Idioma
            pt="Código aberto sob Apache-2.0. Faça fork, rode no seu servidor, leia cada linha. Se este projeto sumir amanhã, o seu site continua de pé."
            en="Open source under Apache-2.0. Fork it, run it on your server, read every line. If this project disappears tomorrow, your site stays up."
          />
        </p>
        <div className="flex flex-wrap justify-center gap-2.5">
          <BotaoProduto href={produto.urlRepo} variante="acc" target="_blank" rel="noopener noreferrer">
            <span aria-hidden="true">★</span> <Idioma pt="Estrelar no GitHub" en="Star on GitHub" />
          </BotaoProduto>
          <BotaoProduto href={produto.urlDocs} target="_blank" rel="noopener noreferrer">
            <Idioma pt="Ler os docs" en="Read the docs" />
          </BotaoProduto>
        </div>
        <div className="mt-[26px] flex flex-wrap justify-center gap-[26px] font-produto-mono text-xs text-produto-faint">
          <span>
            <b className="block text-[19px] font-medium text-produto-brass">{produto.licenca}</b>
            <Idioma pt="licença livre" en="free license" />
          </span>
          <span>
            <b className="block text-[19px] font-medium text-produto-brass">0%</b>
            <Idioma pt="de taxa" en="fees" />
          </span>
          <span>
            <b className="block text-[19px] font-medium text-produto-brass">100%</b>
            <Idioma pt="seu conteúdo" en="your content" />
          </span>
        </div>
      </div>
    </SecaoProduto>
  );
}
