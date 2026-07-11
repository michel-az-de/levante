import { Idioma } from "@/components/Idioma";
import {
  buscarCommitsRecentesGithub,
  buscarIssuesAbertasGithub,
  buscarRepositorioGithub,
} from "@/lib/github/repositorio";
import { produto } from "@/lib/produto";
import type { CommitGithub, IssueGithub } from "@/types/github";
import { CabecalhoProduto, SecaoProduto } from "./SecaoProduto";

const MESES = ["jan", "fev", "mar", "abr", "mai", "jun", "jul", "ago", "set", "out", "nov", "dez"];

function data(iso: string | null): string {
  if (!iso) {
    return "";
  }
  const d = new Date(iso);
  return `${d.getUTCDate()} ${MESES[d.getUTCMonth()]} ${d.getUTCFullYear()}`;
}

function classeLabel(nome: string): string {
  const n = nome.toLowerCase();
  if (n.includes("bug")) {
    return "text-red-400 border-red-900";
  }
  if (n.includes("doc")) {
    return "text-produto-brass border-produto-brass2";
  }
  if (n.includes("enhance") || n.includes("feature")) {
    return "text-produto-jade border-produto-jade/40";
  }
  return "text-produto-dim border-produto-line2";
}

function LinhaIssue({ issue }: { issue: IssueGithub }) {
  return (
    <a
      href={issue.url}
      target="_blank"
      rel="noopener noreferrer"
      className="block border-b border-produto-line px-4 py-3 transition-colors last:border-b-0 hover:bg-produto-bg2"
    >
      <div className="flex items-baseline gap-2">
        <span className="flex-none font-produto-mono text-[11.5px] text-produto-faint">
          #{issue.numero}
        </span>
        <span className="text-[13.5px] leading-tight text-produto-fg">{issue.titulo}</span>
      </div>
      <div className="mt-1.5 flex flex-wrap items-center gap-2 font-produto-mono text-[10.5px] text-produto-faint">
        {issue.labels.slice(0, 3).map((label) => (
          <span key={label} className={`rounded-full border px-2 py-0.5 ${classeLabel(label)}`}>
            {label}
          </span>
        ))}
        {issue.autor ? <span>{issue.autor}</span> : null}
        <span>·</span>
        <span>{data(issue.criadoEm)}</span>
      </div>
    </a>
  );
}

function LinhaCommit({ commit }: { commit: CommitGithub }) {
  return (
    <a
      href={commit.url}
      target="_blank"
      rel="noopener noreferrer"
      className="block border-b border-produto-line px-4 py-3 transition-colors last:border-b-0 hover:bg-produto-bg2"
    >
      <div className="flex items-baseline gap-2.5 text-[13.5px] text-produto-fg">
        <span className="flex-none font-produto-mono text-[11px] text-produto-jade">{commit.sha}</span>
        <span className="leading-tight">{commit.mensagem}</span>
      </div>
      <div className="mt-1.5 font-produto-mono text-[10.5px] text-produto-faint">
        {commit.autor ?? ""} · {data(commit.data)}
      </div>
    </a>
  );
}

function ColunaGithub({
  titulo,
  href,
  hrefLabel,
  children,
}: {
  titulo: React.ReactNode;
  href: string;
  hrefLabel: React.ReactNode;
  children: React.ReactNode;
}) {
  return (
    <div className="overflow-hidden rounded-produto-lg border border-produto-line bg-produto-bg1">
      <div className="flex items-center justify-between border-b border-produto-line bg-produto-bg2 px-4 py-3.5">
        <span className="text-sm font-semibold text-produto-fg">{titulo}</span>
        <a
          href={href}
          target="_blank"
          rel="noopener noreferrer"
          className="font-produto-mono text-[11.5px] text-produto-jade hover:underline"
        >
          {hrefLabel}
        </a>
      </div>
      <div className="flex flex-col">{children}</div>
    </div>
  );
}

/** Secao Codigo: card do repo + issues abertas + commits recentes, ao vivo (E3). */
export async function Repo() {
  const [repo, issues, commits] = await Promise.all([
    buscarRepositorioGithub(produto.repo),
    buscarIssuesAbertasGithub(produto.repo, 5),
    buscarCommitsRecentesGithub(produto.repo, 5),
  ]);

  return (
    <SecaoProduto id="repo">
      <CabecalhoProduto
        kicker={{ pt: "Código", en: "Code" }}
        titulo={{ pt: "Aberto, e vivo.", en: "Open, and alive." }}
        subtitulo={{
          pt: "O repositório, as issues e os últimos commits, direto do GitHub. Levante é mantido à vista de todos.",
          en: "The repository, the issues and the latest commits, straight from GitHub. Levante is maintained in the open.",
        }}
      />

      <a
        href={produto.urlRepo}
        target="_blank"
        rel="noopener noreferrer"
        className="mx-auto mb-4 block max-w-[820px] rounded-produto-lg border border-produto-line2 bg-produto-bg1 p-5 transition-colors hover:border-produto-brass"
      >
        <div className="mb-2.5 flex items-center gap-2.5">
          <span aria-hidden="true" className="h-3.5 w-3.5 rotate-45 border-2 border-produto-brass" />
          <span className="font-produto-mono text-[15px] text-produto-fg">{produto.repo}</span>
          <span
            className={`ml-auto rounded-full border px-2.5 py-0.5 font-produto-mono text-[10.5px] uppercase ${
              repo ? "border-produto-jade/45 text-produto-jade" : "border-produto-line2 text-produto-faint"
            }`}
          >
            {repo ? (
              <Idioma pt="● ao vivo do GitHub" en="● live from GitHub" />
            ) : (
              <Idioma pt="indisponível" en="unavailable" />
            )}
          </span>
        </div>
        <p className="mb-3.5 max-w-[64ch] text-sm text-produto-dim">
          {repo?.descricao ?? (
            <Idioma
              pt="Engine de publicação headless em .NET, API-first e orientada a eventos."
              en="Headless .NET publishing engine, API-first and event-driven."
            />
          )}
        </p>
        <div className="flex flex-wrap gap-4 font-produto-mono text-xs text-produto-faint">
          <span className="flex items-center gap-1.5">
            <i aria-hidden="true" className="h-2.5 w-2.5 rounded-full bg-produto-jade" />
            {repo?.linguagem ?? "C#"}
          </span>
          <span>{repo?.licenca ?? produto.licenca}</span>
          {repo ? <span>★ {repo.estrelas}</span> : null}
          {repo ? <span>⑂ {repo.forks}</span> : null}
          {repo?.atualizadoEm ? (
            <span>
              <Idioma pt="atualizado em " en="updated " />
              {data(repo.atualizadoEm)}
            </span>
          ) : null}
        </div>
      </a>

      <div className="mx-auto grid max-w-[920px] grid-cols-1 gap-3.5 md:grid-cols-2">
        <ColunaGithub
          titulo={<Idioma pt="Issues abertas" en="Open issues" />}
          href={`${produto.urlRepo}/issues`}
          hrefLabel={<Idioma pt="ver todas →" en="view all →" />}
        >
          {issues.length > 0 ? (
            issues.map((issue) => <LinhaIssue key={issue.numero} issue={issue} />)
          ) : (
            <p className="px-4 py-6 text-sm text-produto-faint">
              <Idioma pt="Sem issues abertas no momento." en="No open issues right now." />
            </p>
          )}
        </ColunaGithub>

        <ColunaGithub
          titulo={<Idioma pt="Commits recentes" en="Recent commits" />}
          href={`${produto.urlRepo}/commits`}
          hrefLabel={<Idioma pt="histórico →" en="history →" />}
        >
          {commits.length > 0 ? (
            commits.map((commit) => <LinhaCommit key={commit.sha} commit={commit} />)
          ) : (
            <p className="px-4 py-6 text-sm text-produto-faint">
              <Idioma pt="Histórico indisponível no momento." en="History unavailable right now." />
            </p>
          )}
        </ColunaGithub>
      </div>
    </SecaoProduto>
  );
}
