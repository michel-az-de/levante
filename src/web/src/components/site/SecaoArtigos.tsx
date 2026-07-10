import Link from "next/link";
import { Idioma } from "@/components/Idioma";
import { artigoApi } from "@/lib/api";
import type { Artigo } from "@/types/domain";
import { Botao } from "./Botao";
import { Marca } from "./Marca";
import { CabecalhoSecao, Secao } from "./Secao";

const MESES = [
  "jan",
  "fev",
  "mar",
  "abr",
  "mai",
  "jun",
  "jul",
  "ago",
  "set",
  "out",
  "nov",
  "dez",
];

function formatarData(iso: string | null): string {
  if (!iso) {
    return "";
  }
  const d = new Date(iso);
  return `${d.getUTCDate()} ${MESES[d.getUTCMonth()]} ${d.getUTCFullYear()}`;
}

function tempoLeitura(conteudo: string): number {
  const palavras = conteudo.trim().split(/\s+/).filter(Boolean).length;
  return Math.max(1, Math.round(palavras / 200));
}

async function listar(): Promise<Artigo[]> {
  try {
    const { data } = await artigoApi.GET("/artigos");
    return data ?? [];
  } catch {
    return [];
  }
}

function ArtigoDestaque({ artigo }: { artigo: Artigo }) {
  const tag = artigo.tags[0];
  return (
    <Link
      href={`/artigos/${artigo.slug}`}
      className="group block border-y border-t-site-line2 border-b-site-line py-8"
    >
      <div className="mb-[18px] flex items-center gap-2.5">
        <span className="site-pill">
          <Idioma pt="último" en="latest" />
        </span>
        {tag ? <span className="site-label">{tag}</span> : null}
      </div>
      <h3 className="mb-4 max-w-[22ch] text-[clamp(26px,4vw,46px)] leading-[1.03] font-bold tracking-[-0.025em] text-site-fg transition-colors group-hover:text-site-acc">
        {artigo.titulo}
      </h3>
      <p className="mb-5 max-w-[62ch] text-[16.5px] leading-relaxed text-site-fg2">{artigo.resumo}</p>
      <div className="flex flex-wrap items-center gap-2.5 font-site-mono text-xs text-site-faint">
        <span>{formatarData(artigo.dataPublicacao)}</span>
        <span>·</span>
        <span>{tempoLeitura(artigo.conteudo)} min</span>
        <span>·</span>
        <span className="flex items-center gap-1.5 text-site-acc">
          <Idioma pt="Ler" en="Read" /> <span aria-hidden="true">→</span>
        </span>
      </div>
    </Link>
  );
}

function LinhaArtigo({ artigo, numero }: { artigo: Artigo; numero: number }) {
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

/** Secao 04 — artigos (destaque + lista), dados reais da Content API. */
export async function SecaoArtigos() {
  const artigos = await listar();
  const destaque = artigos[0];
  const resto = artigos.slice(1, 6);

  return (
    <Secao id="artigos">
      <CabecalhoSecao
        numero="04"
        kicker={{ pt: "escrita", en: "writing" }}
        titulo={{ pt: "Artigos", en: "Writing" }}
        subtitulo={{
          pt: "Como eu penso arquitetura, .NET e sistemas a eventos. Publicado no Levante.",
          en: "How I think about architecture, .NET and event-driven systems. Published on Levante.",
        }}
        acao={
          <Botao href="/artigos" tamanho="sm">
            <Idioma pt="Todos os artigos" en="All articles" /> <span aria-hidden="true">→</span>
          </Botao>
        }
      />

      {destaque ? (
        <ArtigoDestaque artigo={destaque} />
      ) : (
        <p className="py-6 text-site-faint">
          <Idioma pt="Nada publicado ainda." en="Nothing published yet." />
        </p>
      )}

      <div>
        {resto.map((artigo, i) => (
          <LinhaArtigo key={artigo.slug} artigo={artigo} numero={i + 2} />
        ))}
      </div>

      <div className="mt-8 flex flex-wrap items-center justify-between gap-3.5">
        <span className="flex items-center gap-2 font-site-mono text-xs text-site-faint">
          <Marca className="h-3.5 w-3.5" />
          <Idioma
            pt="servido pela Content API do Levante"
            en="served by the Levante Content API"
          />
        </span>
        <Link
          href="/newsletter"
          className="font-site-mono text-[13px] text-site-fg2 transition-colors hover:text-site-acc"
        >
          <Idioma pt="Assinar a newsletter" en="Subscribe" /> →
        </Link>
      </div>
    </Secao>
  );
}
