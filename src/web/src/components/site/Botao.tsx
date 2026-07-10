"use client";

import type { AnchorHTMLAttributes, ReactNode } from "react";
import { useMagnetic } from "@/lib/hooks/useMagnetic";

type Variante = "padrao" | "acc";
type Tamanho = "md" | "sm";

const BASE = "inline-flex items-center gap-2 border font-medium transition-colors";
const TAMANHOS: Record<Tamanho, string> = {
  md: "px-[19px] py-3 text-sm",
  sm: "px-3.5 py-2.5 text-[13px]",
};
const VARIANTES: Record<Variante, string> = {
  padrao: "border-site-line2 text-site-fg hover:border-site-fg hover:bg-site-fg hover:text-site-bg",
  acc: "border-site-acc bg-site-acc text-site-onacc hover:bg-transparent hover:text-site-acc",
};

/** Link estilizado do site. `magnetico` liga o hover que acompanha o cursor. */
export function Botao({
  children,
  variante = "padrao",
  tamanho = "md",
  magnetico = false,
  className,
  ...props
}: {
  children: ReactNode;
  variante?: Variante;
  tamanho?: Tamanho;
  magnetico?: boolean;
  className?: string;
} & AnchorHTMLAttributes<HTMLAnchorElement>) {
  const ref = useMagnetic<HTMLAnchorElement>();
  const classe = `${BASE} ${TAMANHOS[tamanho]} ${VARIANTES[variante]}${className ? ` ${className}` : ""}`;
  return (
    <a ref={magnetico ? ref : undefined} className={classe} {...props}>
      {children}
    </a>
  );
}
