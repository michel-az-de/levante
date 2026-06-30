import ReactMarkdown from "react-markdown";
import remarkGfm from "remark-gfm";

/**
 * Renderiza markdown como elementos React (sem HTML cru -> seguro contra injecao).
 * Sem "use client": serve tanto no SSR do publico quanto no preview do admin.
 * remark-gfm habilita tabelas, listas de tarefas e autolink (GitHub-flavored).
 */
export function Markdown({ children }: { children: string }) {
  return (
    <div className="markdown">
      <ReactMarkdown remarkPlugins={[remarkGfm]}>{children}</ReactMarkdown>
    </div>
  );
}
