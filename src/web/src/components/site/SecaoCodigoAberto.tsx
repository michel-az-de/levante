import { lerConfigGithub } from "@/lib/github/config";
import { buscarContribuicoesGithub } from "@/lib/github/contribuicoes";
import { buscarRepositorioGithub } from "@/lib/github/repositorio";
import { reposVitrine } from "@/lib/site-conteudo";
import { HeatmapContribuicoes } from "./HeatmapContribuicoes";
import { RepositorioCard } from "./RepositorioCard";
import { RevealOnScroll } from "./RevealOnScroll";
import { CabecalhoSecao, Secao } from "./Secao";

/** Secao 03 — codigo aberto (bento com heatmap ao vivo + cards de repo). */
export async function SecaoCodigoAberto() {
  const { perfil } = lerConfigGithub();
  const [calendario, lives] = await Promise.all([
    buscarContribuicoesGithub(perfil),
    Promise.all(reposVitrine.map((repo) => buscarRepositorioGithub(repo.chave))),
  ]);

  return (
    <Secao id="github">
      <CabecalhoSecao
        numero="03"
        kicker={{ pt: "código", en: "code" }}
        titulo={{ pt: "Código aberto", en: "Open source" }}
        subtitulo={{
          pt: "A prova pública do método. O que eu mantenho no GitHub.",
          en: "The public proof of the method. What I keep on GitHub.",
        }}
        acao={
          <a
            href={`https://github.com/${perfil}`}
            target="_blank"
            rel="noopener noreferrer"
            className="font-site-mono text-[13px] text-site-fg2 transition-colors hover:text-site-acc"
          >
            github.com/{perfil} →
          </a>
        }
      />

      <RevealOnScroll className="grid grid-cols-1 gap-px border border-site-line bg-site-line sm:grid-cols-2 lg:grid-cols-[1.35fr_1fr_1fr]">
        <div className="flex min-h-[150px] flex-col bg-site-bg p-6 sm:col-span-2 lg:col-span-1 lg:row-span-2">
          <HeatmapContribuicoes calendario={calendario} />
        </div>
        {reposVitrine.map((repo, i) => (
          <RepositorioCard key={repo.chave} repo={repo} live={lives[i]} />
        ))}
      </RevealOnScroll>
    </Secao>
  );
}
