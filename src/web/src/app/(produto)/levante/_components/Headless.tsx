import type { ReactNode } from "react";
import Link from "next/link";
import { Idioma } from "@/components/Idioma";
import { CabecalhoProduto, SecaoProduto } from "./SecaoProduto";

function FNode({
  titulo,
  sub,
  tom,
}: {
  titulo: ReactNode;
  sub: string;
  tom?: "eng" | "you";
}) {
  const borda =
    tom === "you"
      ? "border-produto-jade bg-produto-jadeb"
      : tom === "eng"
        ? "border-produto-brass2 bg-produto-bg2"
        : "border-produto-line2 bg-produto-bg2";
  const cor = tom === "you" ? "text-produto-jade" : "text-produto-fg";
  return (
    <div className={`min-w-[118px] rounded-produto-md border px-4 py-3.5 text-center ${borda}`}>
      <div className={`text-sm font-semibold ${cor}`}>{titulo}</div>
      <div className="mt-0.5 font-produto-mono text-[10.5px] text-produto-dim">{sub}</div>
    </div>
  );
}

function Seta() {
  return (
    <span aria-hidden="true" className="flex-none text-lg text-produto-brass">
      →
    </span>
  );
}

/** Secao Headless: fluxo Editor -> Levante -> Content API -> seu front-end. */
export function Headless() {
  return (
    <SecaoProduto id="headless">
      <CabecalhoProduto
        kicker={{ pt: "Headless", en: "Headless" }}
        titulo={{
          pt: "A engine publica. Seu site decide como aparecer.",
          en: "The engine publishes. Your site decides how it looks.",
        }}
        subtitulo={{
          pt: "Levante não te prende a um tema. Ele serve o conteúdo por API e você monta o front-end que quiser.",
          en: "Levante doesn't lock you into a theme. It serves content via API and you build whatever front-end you want.",
        }}
      />
      <div className="mx-auto max-w-[920px] rounded-produto-lg border border-produto-line2 bg-produto-bg1 p-[clamp(20px,4vw,34px)]">
        <div className="flex flex-wrap items-center justify-center gap-2.5">
          <FNode titulo={<Idioma pt="Editor" en="Editor" />} sub="admin" />
          <Seta />
          <FNode titulo="Levante" sub=".NET · engine" tom="eng" />
          <Seta />
          <FNode titulo="Content API" sub="OpenAPI" />
          <Seta />
          <FNode titulo={<Idioma pt="Seu front-end" en="Your front-end" />} sub="Next.js · React" tom="you" />
        </div>
        <div className="mt-3 flex flex-col items-center gap-2.5">
          <span className="font-produto-mono text-[10.5px] uppercase tracking-[0.08em] text-produto-faint">
            <Idioma pt="núcleo de eventos" en="event core" />
          </span>
          <div className="flex flex-wrap items-center justify-center gap-2.5">
            <FNode titulo="Outbox" sub="Mongo" />
            <Seta />
            <FNode titulo={<Idioma pt="Fila" en="Queue" />} sub="RabbitMQ" />
            <Seta />
            <FNode titulo={<Idioma pt="Sua notificação" en="Your notifications" />} sub="email · push" />
          </div>
        </div>
        <p className="mt-[18px] text-center text-[13px] text-produto-dim">
          <Idioma
            pt="É assim que o site do Felipe funciona: um front-end Next.js consumindo a API do Levante. "
            en="This is how Felipe's site works: a Next.js front-end consuming the Levante API. "
          />
          <Link href="/" className="border-b border-dotted border-produto-jade text-produto-jade">
            <Idioma pt="ver em produção →" en="see it live →" />
          </Link>
        </p>
      </div>
    </SecaoProduto>
  );
}
