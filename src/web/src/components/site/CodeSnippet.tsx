import type { ReactNode } from "react";
import { Marca } from "./Marca";

/**
 * Bloco de codigo com barra de arquivo. O conteudo (children) ja vem colorido
 * com as classes de sintaxe (text-site-kw, text-site-ty, etc.).
 */
export function CodeSnippet({ arquivo, children }: { arquivo: ReactNode; children: ReactNode }) {
  return (
    <div className="border border-site-line2 bg-site-bg1">
      <div className="flex items-center gap-2.5 border-b border-site-line px-4 py-3 font-site-mono text-[11.5px] text-site-faint">
        <Marca className="h-3 w-3" />
        {arquivo}
      </div>
      <pre className="overflow-x-auto p-[18px] font-site-mono text-[13px] leading-[1.75] text-site-fg">
        {children}
      </pre>
    </div>
  );
}
