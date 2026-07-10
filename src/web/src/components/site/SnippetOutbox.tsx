import { Fragment } from "react";
import { Idioma } from "@/components/Idioma";
import { CodeSnippet } from "./CodeSnippet";

type Token = { txt: string; cor?: string };

// Snippet do OutboxHandler com realce de sintaxe manual (tokens). O texto entre
// tokens carrega os \n; o <pre> do CodeSnippet preserva o whitespace.
const CODIGO: readonly Token[] = [
  { txt: "// publicar grava o estado e o evento juntos, ou nenhum\n", cor: "text-site-cm" },
  { txt: "public async ", cor: "text-site-kw" },
  { txt: "Task", cor: "text-site-ty" },
  { txt: "<" },
  { txt: "Result", cor: "text-site-ty" },
  { txt: "> " },
  { txt: "Handle", cor: "text-site-fn" },
  { txt: "(" },
  { txt: "PublicarArtigo", cor: "text-site-ty" },
  { txt: " cmd)\n{\n    artigo." },
  { txt: "Publicar", cor: "text-site-fn" },
  { txt: "();\n    " },
  { txt: "await", cor: "text-site-kw" },
  { txt: " _repo." },
  { txt: "SalvarAsync", cor: "text-site-fn" },
  { txt: "(artigo);\n    " },
  { txt: "await", cor: "text-site-kw" },
  { txt: " _outbox." },
  { txt: "EscreverAsync", cor: "text-site-fn" },
  { txt: "(" },
  { txt: "new", cor: "text-site-kw" },
  { txt: " " },
  { txt: "ArtigoPublicado", cor: "text-site-ty" },
  { txt: "(artigo.Id));\n    " },
  { txt: "// relay -> fila -> entrega. nada se perde.\n    ", cor: "text-site-cm" },
  { txt: "return", cor: "text-site-kw" },
  { txt: " " },
  { txt: "Result", cor: "text-site-ty" },
  { txt: "." },
  { txt: "Ok", cor: "text-site-fn" },
  { txt: "();\n}" },
];

/** Snippet do handler de publicacao com outbox (secao Capacidades). */
export function SnippetOutbox() {
  return (
    <CodeSnippet
      arquivo={
        <>
          OutboxHandler.cs ·{" "}
          <Idioma
            pt="estado e evento na mesma transação"
            en="state and event in the same transaction"
          />
        </>
      }
    >
      {CODIGO.map((token, i) =>
        token.cor ? (
          <span key={i} className={token.cor}>
            {token.txt}
          </span>
        ) : (
          <Fragment key={i}>{token.txt}</Fragment>
        ),
      )}
    </CodeSnippet>
  );
}
