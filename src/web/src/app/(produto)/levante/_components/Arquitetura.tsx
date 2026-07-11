import { Fragment } from "react";
import type { Bilingue } from "@/lib/i18n/textos";
import { Idioma } from "@/components/Idioma";
import { SecaoProduto } from "./SecaoProduto";

const principios: readonly { rotulo: Bilingue; texto: Bilingue }[] = [
  {
    rotulo: { pt: "Atomicidade:", en: "Atomicity:" },
    texto: {
      pt: "ou estado e evento entram juntos, ou nenhum",
      en: "state and event commit together, or neither",
    },
  },
  {
    rotulo: { pt: "Desacoplamento:", en: "Decoupling:" },
    texto: {
      pt: "a publicação não conhece quem entrega",
      en: "publishing doesn't know who delivers",
    },
  },
  {
    rotulo: { pt: "Resiliência:", en: "Resilience:" },
    texto: {
      pt: "dead letter e reprocessamento na fila",
      en: "dead letter and reprocessing on the queue",
    },
  },
];

type Token = { txt: string; cor?: string };

const CODIGO: readonly Token[] = [
  { txt: "// publicar grava artigo + evento juntos\n", cor: "text-produto-faint" },
  { txt: "public async ", cor: "text-produto-brass" },
  { txt: "Task", cor: "text-produto-jade" },
  { txt: "<" },
  { txt: "Result", cor: "text-produto-jade" },
  { txt: "> " },
  { txt: "Handle", cor: "text-produto-jade" },
  { txt: "(" },
  { txt: "PublicarArtigo", cor: "text-produto-jade" },
  { txt: " cmd)\n{\n    artigo." },
  { txt: "Publicar", cor: "text-produto-jade" },
  { txt: "();\n    " },
  { txt: "await", cor: "text-produto-brass" },
  { txt: " _repo." },
  { txt: "SalvarAsync", cor: "text-produto-jade" },
  { txt: "(artigo);\n    " },
  { txt: "await", cor: "text-produto-brass" },
  { txt: " _outbox." },
  { txt: "EscreverAsync", cor: "text-produto-jade" },
  { txt: "(\n        " },
  { txt: "new", cor: "text-produto-brass" },
  { txt: " " },
  { txt: "ArtigoPublicado", cor: "text-produto-jade" },
  { txt: "(artigo.Id));\n    " },
  { txt: "// 1 transação. relay → fila → entrega\n    ", cor: "text-produto-faint" },
  { txt: "return", cor: "text-produto-brass" },
  { txt: " " },
  { txt: "Result", cor: "text-produto-jade" },
  { txt: ".Ok();\n}" },
];

/** Secao Arquitetura: principios + snippet do handler de outbox. */
export function Arquitetura() {
  return (
    <SecaoProduto id="arquitetura">
      <div className="mx-auto grid max-w-[980px] grid-cols-1 items-center gap-8 md:grid-cols-2">
        <div>
          <div className="mb-3 font-produto-mono text-xs uppercase tracking-[0.12em] text-produto-jade">
            <Idioma pt="Arquitetura" en="Architecture" />
          </div>
          <h3 className="mb-3.5 text-[22px] font-bold tracking-tight text-produto-fg">
            <Idioma pt="Orientado a eventos, do jeito certo." en="Event-driven, done right." />
          </h3>
          <p className="mb-3.5 text-[15px] leading-[1.6] text-produto-dim">
            <Idioma
              pt="O estado e o evento são gravados na mesma transação. Um relay observa o outbox e publica na fila. A API nunca chama notificação direto."
              en="State and event are written in the same transaction. A relay watches the outbox and publishes to the queue. The API never calls notifications directly."
            />
          </p>
          <ul className="flex flex-col gap-2.5">
            {principios.map((principio) => (
              <li key={principio.rotulo.pt} className="relative pl-6 text-sm text-produto-fg">
                <span aria-hidden="true" className="absolute top-[3px] left-0 text-[11px] text-produto-jade">
                  ◆
                </span>
                <b className="font-medium text-produto-dim">
                  <Idioma pt={principio.rotulo.pt} en={principio.rotulo.en} />
                </b>{" "}
                <Idioma pt={principio.texto.pt} en={principio.texto.en} />
              </li>
            ))}
          </ul>
        </div>

        <div className="overflow-hidden rounded-produto-lg border border-produto-line2 bg-produto-bg1">
          <div className="flex gap-1.5 border-b border-produto-line bg-produto-bg2 px-3.5 py-[11px]">
            <i aria-hidden="true" className="h-[11px] w-[11px] rounded-full bg-produto-line2" />
            <i aria-hidden="true" className="h-[11px] w-[11px] rounded-full bg-produto-line2" />
            <i aria-hidden="true" className="h-[11px] w-[11px] rounded-full bg-produto-line2" />
          </div>
          <pre className="overflow-x-auto p-4 font-produto-mono text-[12.5px] leading-[1.75] text-produto-fg">
            {CODIGO.map((token, i) =>
              token.cor ? (
                <span key={i} className={token.cor}>
                  {token.txt}
                </span>
              ) : (
                <Fragment key={i}>{token.txt}</Fragment>
              ),
            )}
          </pre>
        </div>
      </div>
    </SecaoProduto>
  );
}
