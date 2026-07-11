import { Children, isValidElement, type ReactNode } from "react";
import ReactMarkdown from "react-markdown";
import remarkGfm from "remark-gfm";
import { slugificar } from "@/lib/artigos";

/** Flatten do conteudo de uma heading para derivar o id de ancora. */
function textoDe(node: ReactNode): string {
  return Children.toArray(node)
    .map((filho) => {
      if (typeof filho === "string" || typeof filho === "number") {
        return String(filho);
      }
      if (isValidElement(filho)) {
        return textoDe((filho.props as { children?: ReactNode }).children);
      }
      return "";
    })
    .join("");
}

/**
 * Renderiza markdown como elementos React (sem HTML cru -> seguro contra injecao).
 * Sem "use client": serve tanto no SSR do publico quanto no preview do admin.
 * remark-gfm habilita tabelas, listas de tarefas e autolink (GitHub-flavored).
 * As headings ganham id (slug do texto) para ancorar o TOC lateral do artigo.
 */
export function Markdown({ children }: { children: string }) {
  return (
    <div className="markdown">
      <ReactMarkdown
        remarkPlugins={[remarkGfm]}
        components={{
          h2: ({ children }) => <h2 id={slugificar(textoDe(children))}>{children}</h2>,
          h3: ({ children }) => <h3 id={slugificar(textoDe(children))}>{children}</h3>,
        }}
      >
        {children}
      </ReactMarkdown>
    </div>
  );
}
