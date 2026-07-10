// Fetchers REST do GitHub: metadados de repositorio, issues abertas e commits
// recentes (fatia E3, ADR 0006). TTL de 1h. Resiliencia honesta: qualquer falha
// (rede, rate limit, 5xx) degrada para null/[] — nunca dado inventado.

import type { CommitGithub, IssueGithub, RepositorioGithub } from "@/types/github";
import { githubRest } from "./client";
import { lerConfigGithub } from "./config";

const TTL = 3600;

type RepoBruto = {
  full_name: string;
  description: string | null;
  language: string | null;
  license: { spdx_id: string | null } | null;
  stargazers_count: number;
  forks_count: number;
  pushed_at: string | null;
  html_url: string;
};

export async function buscarRepositorioGithub(repo: string): Promise<RepositorioGithub | null> {
  const { token } = lerConfigGithub();
  try {
    const bruto = await githubRest<RepoBruto>(`/repos/${repo}`, TTL, token);
    return {
      nomeCompleto: bruto.full_name,
      descricao: bruto.description,
      linguagem: bruto.language,
      licenca: bruto.license?.spdx_id ?? null,
      estrelas: bruto.stargazers_count,
      forks: bruto.forks_count,
      atualizadoEm: bruto.pushed_at,
      url: bruto.html_url,
    };
  } catch {
    return null;
  }
}

type IssueBruta = {
  number: number;
  title: string;
  labels: ({ name: string } | string)[];
  user: { login: string } | null;
  created_at: string;
  html_url: string;
  // O endpoint /issues devolve PRs tambem; PRs carregam este campo.
  pull_request?: unknown;
};

export async function buscarIssuesAbertasGithub(
  repo: string,
  limite = 5,
): Promise<IssueGithub[]> {
  const { token } = lerConfigGithub();
  try {
    // Pede a mais porque parte do retorno pode ser PR (filtrado abaixo).
    const brutas = await githubRest<IssueBruta[]>(
      `/repos/${repo}/issues?state=open&per_page=${limite + 5}`,
      TTL,
      token,
    );
    return brutas
      .filter((issue) => issue.pull_request === undefined)
      .slice(0, limite)
      .map((issue) => ({
        numero: issue.number,
        titulo: issue.title,
        labels: issue.labels.map((label) => (typeof label === "string" ? label : label.name)),
        autor: issue.user?.login ?? null,
        criadoEm: issue.created_at,
        url: issue.html_url,
      }));
  } catch {
    return [];
  }
}

type CommitBruto = {
  sha: string;
  commit: { message: string; author: { name: string | null; date: string | null } | null };
  author: { login: string } | null;
  html_url: string;
};

export async function buscarCommitsRecentesGithub(
  repo: string,
  limite = 5,
): Promise<CommitGithub[]> {
  const { token } = lerConfigGithub();
  try {
    const brutos = await githubRest<CommitBruto[]>(
      `/repos/${repo}/commits?per_page=${limite}`,
      TTL,
      token,
    );
    return brutos.map((item) => ({
      sha: item.sha.slice(0, 7),
      mensagem: (item.commit.message.split("\n")[0] ?? "").trim(),
      autor: item.commit.author?.name ?? item.author?.login ?? null,
      data: item.commit.author?.date ?? null,
      url: item.html_url,
    }));
  } catch {
    return [];
  }
}
