import type { AnchorHTMLAttributes, ReactNode } from "react";

type Variante = "padrao" | "acc";

const BASE =
  "inline-flex items-center gap-2 rounded-produto-md border px-4 py-2.5 text-[13.5px] font-semibold transition-[filter,border-color]";
const VARIANTES: Record<Variante, string> = {
  padrao: "border-produto-line2 text-produto-fg hover:border-produto-brass",
  acc: "border-produto-jade bg-produto-jade text-produto-jadetx hover:brightness-110",
};

/** Link estilizado da landing do produto (marca brass/jade, cantos arredondados). */
export function BotaoProduto({
  children,
  variante = "padrao",
  className,
  ...props
}: {
  children: ReactNode;
  variante?: Variante;
  className?: string;
} & AnchorHTMLAttributes<HTMLAnchorElement>) {
  return (
    <a
      className={`${BASE} ${VARIANTES[variante]}${className ? ` ${className}` : ""}`}
      {...props}
    >
      {children}
    </a>
  );
}
