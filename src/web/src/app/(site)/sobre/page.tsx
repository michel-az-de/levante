import type { Metadata } from "next";
import Link from "next/link";
import { Idioma } from "@/components/Idioma";
import { site } from "@/lib/site";

const EMAIL = "felipe.azevedoit@gmail.com";
const LINKEDIN = "https://linkedin.com/in/felipe-azevedo-05493357";
const GITHUB = "https://github.com/felipeazevedoit";

export const metadata: Metadata = {
  title: "Sobre",
  description:
    "Felipe Michel de Azevedo: arquiteto de soluções e desenvolvedor sênior .NET / full stack. Sistemas de negócio do modelo de domínio à entrega em produção.",
  alternates: { canonical: "/sobre" },
  openGraph: { title: "Sobre", url: `${site.url}/sobre` },
};

const linkMono =
  "text-site-fg2 transition-colors hover:text-site-acc";

export default function SobrePage() {
  return (
    <div className="mx-auto max-w-[760px] px-[clamp(18px,4vw,40px)] py-[clamp(48px,8vw,92px)]">
      <Link
        href="/"
        className="inline-flex items-center gap-2 font-site-mono text-[12.5px] text-site-fg2 transition-colors hover:text-site-acc"
      >
        ← <Idioma pt="Voltar ao início" en="Back home" />
      </Link>

      <h1 className="mt-6 mb-2 text-[clamp(32px,5vw,52px)] leading-[1.05] font-bold tracking-[-0.03em] text-site-fg">
        Sobre
      </h1>
      <p className="font-site-mono text-[12.5px] text-site-faint">{site.jobTitle}</p>

      {/* TODO(felipe): revisar/personalizar a bio abaixo. Mantida factual e
          conservadora, ancorada só no que já está no repo (papel, foco técnico,
          ethos do projeto) — sem inventar empresas, anos ou feitos. */}
      <div className="mt-10 flex flex-col gap-4 text-[16.5px] leading-[1.7] text-site-fg2">
        <p>
          Sou o <strong className="text-site-fg">Felipe</strong> — arquiteto de soluções e
          desenvolvedor sênior, com foco em <strong className="text-site-fg">.NET</strong> e full
          stack. Construo sistemas de negócio de ponta a ponta: do modelo de domínio à entrega em
          produção.
        </p>
        <p>
          Meu trabalho gira em torno de arquitetura bem desenhada — Domain-Driven Design, Clean
          Architecture e SOLID como fundação, não como enfeite. Gosto de código que expressa o
          negócio na linguagem de quem o vive, e de decisões técnicas que se sustentam quando o
          sistema cresce.
        </p>
        <p>
          Trato engenharia como ofício. Este próprio site é peça de portfólio: o padrão de
          construção importa tanto quanto a funcionalidade — da pedra bruta à pedra polida.
        </p>
        <p>
          Aqui eu escrevo sobre o que aprendo — arquitetura, .NET e as decisões por trás de sistemas
          reais — e mostro os projetos em que trabalho. Comece pelos{" "}
          <Link href="/artigos" className="text-site-acc underline-offset-2 hover:underline">
            artigos
          </Link>
          .
        </p>
      </div>

      <div className="mt-12 flex flex-wrap gap-[26px] border-t border-site-line pt-6 font-site-mono text-[13px]">
        <a href={`mailto:${EMAIL}`} className={linkMono}>
          {EMAIL}
        </a>
        <a href={LINKEDIN} target="_blank" rel="noopener noreferrer" className={linkMono}>
          linkedin ↗
        </a>
        <a href={GITHUB} target="_blank" rel="noopener noreferrer" className={linkMono}>
          github ↗
        </a>
      </div>
    </div>
  );
}
