import Link from "next/link";
import { formatarData, tempoLeitura } from "@/lib/artigos";
import type { Artigo } from "@/types/domain";

/** Linha de artigo estilo indice (home e lista de artigos). */
export function LinhaArtigo({ artigo, numero }: { artigo: Artigo; numero: number }) {
  return (
    <Link
      href={`/artigos/${artigo.slug}`}
      className="group grid grid-cols-[auto_1fr] items-baseline gap-x-[22px] gap-y-1 border-b border-site-line py-[21px] transition-[padding] hover:pl-3.5 md:grid-cols-[auto_1fr_auto_auto]"
    >
      <span className="font-site-mono text-[13px] text-site-faint group-hover:text-site-acc">
        {String(numero).padStart(2, "0")}
      </span>
      <span className="text-[clamp(18px,2.3vw,25px)] leading-tight font-semibold tracking-tight text-site-fg group-hover:text-site-acc">
        {artigo.titulo}
      </span>
      <span className="site-label col-start-2 md:col-start-auto">{artigo.tags[0] ?? ""}</span>
      <span className="col-start-2 font-site-mono text-[12.5px] whitespace-nowrap text-site-faint md:col-start-auto">
        {formatarData(artigo.dataPublicacao)} · {tempoLeitura(artigo.conteudo)} min
      </span>
    </Link>
  );
}
