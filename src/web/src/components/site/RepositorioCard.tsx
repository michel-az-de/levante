import { Idioma } from "@/components/Idioma";
import type { RepoVitrine } from "@/lib/site-conteudo";
import type { RepositorioGithub } from "@/types/github";

/**
 * Card de repositorio do bento GitHub. Descricao/nome sao curados (site-conteudo);
 * linguagem/licenca/estrelas vem ao vivo de lib/github quando ha token, senao
 * caem nos valores curados (degradacao honesta — ADR 0006).
 */
export function RepositorioCard({
  repo,
  live,
}: {
  repo: RepoVitrine;
  live: RepositorioGithub | null;
}) {
  const linguagem = live?.linguagem ?? repo.linguagem;
  const licenca = live?.licenca ?? repo.licenca ?? null;
  const url = live?.url ?? `https://github.com/${repo.chave}`;

  return (
    <a
      href={url}
      target="_blank"
      rel="noopener noreferrer"
      className="group flex min-h-[150px] flex-col bg-site-bg p-6 transition-colors hover:bg-site-bg1"
    >
      <div className="mb-2.5 flex items-center gap-2 font-site-mono text-[15px] font-medium text-site-fg group-hover:text-site-acc">
        {repo.nome}
        <span className="ml-auto text-site-faint transition-transform group-hover:translate-x-0.5">
          ↗
        </span>
      </div>
      <p className="mb-auto text-[13.5px] leading-relaxed text-site-fg2">
        <Idioma pt={repo.descricao.pt} en={repo.descricao.en} />
      </p>
      <div className="mt-4 flex items-center gap-3.5 font-site-mono text-[11px] text-site-faint">
        <span className="flex items-center gap-1.5">
          <i aria-hidden="true" className="h-2 w-2 rounded-full bg-site-acc" />
          {linguagem}
        </span>
        {licenca ? <span>{licenca}</span> : null}
        {live ? <span>★ {live.estrelas}</span> : null}
      </div>
    </a>
  );
}
