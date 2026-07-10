import { Idioma } from "@/components/Idioma";

/**
 * Card do autor (Felipe). Neutro de proposito: nao herda a marca do produto nem
 * a pessoal — o Felipe nao e uma feature do Levante, e o Levante e uma coisa que
 * o Felipe mantem. Reutilizavel em outras superficies.
 */
export function AutorCard({ mailtoHref, githubUrl }: { mailtoHref: string; githubUrl: string }) {
  return (
    <div className="mx-auto flex max-w-[900px] flex-col gap-6 rounded-2xl border border-neutral-700/60 bg-neutral-900 p-8 text-neutral-200 sm:flex-row sm:gap-7">
      <div className="flex h-20 w-20 flex-none items-center justify-center rounded-2xl border border-neutral-600 bg-neutral-800 text-2xl font-bold tracking-wide text-neutral-300">
        FA
      </div>
      <div>
        <div className="mb-2 font-mono text-xs uppercase tracking-wider text-neutral-500">
          <Idioma pt="O autor" en="The author" />
        </div>
        <h2 className="mb-3 text-2xl font-bold text-neutral-100">
          <Idioma pt="Feito por Felipe Azevedo." en="Built by Felipe Azevedo." />
        </h2>
        <p className="mb-3 text-sm leading-relaxed text-neutral-400">
          <Idioma
            pt="Arquiteto de software com quinze anos em .NET e Azure, a maior parte em sistemas bancários e financeiros de missão crítica."
            en="Software architect with fifteen years in .NET and Azure, mostly in mission-critical banking and financial systems."
          />
        </p>
        <p className="mb-5 text-sm leading-relaxed text-neutral-400">
          <Idioma
            pt="Cada decisão do Levante, do outbox transacional à Content API, vem de quem já colocou sistemas assim em produção, onde não dá pra errar. É a mesma engenharia que eu levo pra um squad sênior."
            en="Every Levante decision, from the transactional outbox to the Content API, comes from someone who has shipped systems like this to production, where you can't get it wrong. The same engineering I bring to a senior squad."
          />
        </p>
        <div className="flex flex-wrap gap-3">
          <a
            href={mailtoHref}
            className="rounded-lg bg-neutral-100 px-4 py-2 text-sm font-semibold text-neutral-900 transition hover:bg-white"
          >
            <Idioma pt="Falar com o Felipe" en="Talk to Felipe" />
          </a>
          <a
            href={githubUrl}
            target="_blank"
            rel="noopener noreferrer"
            className="rounded-lg border border-neutral-600 px-4 py-2 text-sm font-semibold transition hover:border-neutral-400"
          >
            <Idioma pt="GitHub do autor" en="Author's GitHub" />
          </a>
        </div>
      </div>
    </div>
  );
}
