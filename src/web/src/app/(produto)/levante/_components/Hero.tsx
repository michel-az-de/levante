import { Idioma } from "@/components/Idioma";
import { produto } from "@/lib/produto";
import { BotaoProduto } from "./BotaoProduto";
import { CodeTabs, type Aba } from "./CodeTabs";

const abas: readonly Aba[] = [
  {
    id: "docker",
    rotulo: "docker",
    comando: `docker run -p 8080:8080 ${produto.imagemDocker}`,
  },
  {
    id: "source",
    rotulo: "source",
    comando: `git clone ${produto.cloneUrl}\ncd levante && dotnet run --project ${produto.projetoApi}`,
  },
];

/** Hero da landing do produto. */
export function Hero() {
  return (
    <header className="px-[clamp(16px,4vw,32px)] pt-[clamp(54px,9vw,104px)] pb-[clamp(30px,5vw,56px)] text-center">
      <div className="mx-auto max-w-[1140px]">
        <span className="mb-[22px] inline-flex items-center gap-2.5 rounded-full border border-produto-line2 px-3.5 py-1.5 font-produto-mono text-xs uppercase tracking-[0.12em] text-produto-brass">
          <span aria-hidden="true" className="h-[5px] w-[5px] rounded-full bg-produto-jade" />
          open source · {produto.dotnet.toLowerCase()} · headless
        </span>
        <h1 className="mx-auto mb-5 max-w-[16ch] text-[clamp(36px,6.5vw,68px)] leading-[1.03] font-bold tracking-[-0.025em] text-produto-fg">
          <Idioma
            pt={
              <>
                A plataforma de publicação para quem{" "}
                <span className="text-produto-jade">constrói</span>.
              </>
            }
            en={
              <>
                The publishing platform for people who{" "}
                <span className="text-produto-jade">build</span>.
              </>
            }
          />
        </h1>
        <p className="mx-auto mb-3.5 max-w-[60ch] text-[clamp(16px,2.1vw,20px)] leading-[1.55] text-produto-dim">
          <Idioma
            pt={
              <>
                Levante é uma engine de publicação{" "}
                <b className="font-medium text-produto-fg">headless</b> em .NET. API-first, núcleo
                orientado a eventos, self-hostable. Seu conteúdo, seu domínio, seu stack.
              </>
            }
            en={
              <>
                Levante is a <b className="font-medium text-produto-fg">headless</b> .NET publishing
                engine. API-first, event-driven core, self-hostable. Your content, your domain, your
                stack.
              </>
            }
          />
        </p>
        <p className="mb-[30px] font-produto-mono text-[12.5px] tracking-[0.04em] text-produto-brass">
          {"// da pedra bruta à pedra polida"}
        </p>
        <div className="flex flex-wrap justify-center gap-2.5">
          <BotaoProduto href={produto.urlRepo} variante="acc" target="_blank" rel="noopener noreferrer">
            <span aria-hidden="true">★</span>{" "}
            <Idioma pt="Ver no GitHub" en="View on GitHub" />
          </BotaoProduto>
          <BotaoProduto href={produto.urlDocs} target="_blank" rel="noopener noreferrer">
            <Idioma pt="Documentação" en="Documentation" />
          </BotaoProduto>
        </div>
        <div className="mt-[18px] flex flex-wrap justify-center gap-4 font-produto-mono text-[11.5px] text-produto-faint">
          <span>
            <b className="font-medium text-produto-dim">{produto.licenca}</b>{" "}
            <Idioma pt="licença" en="license" />
          </span>
          <span>
            <b className="font-medium text-produto-dim">{produto.dotnet}</b> · Minimal API
          </span>
          <span>
            <b className="font-medium text-produto-dim">self-host</b>{" "}
            <Idioma pt="ou gerenciado" en="or managed" />
          </span>
        </div>
        <CodeTabs abas={abas} />
      </div>
    </header>
  );
}
