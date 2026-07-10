// Primitivas de acesso a API do GitHub (fatia E3, ADR 0006). REST v3 para
// repositorio/issues/commits (publicos, mas autenticamos para o rate limit alto)
// e GraphQL v4 para o calendario de contribuicoes (exige auth). O cache e por
// fetch (`next.revalidate`), pois heatmap e cards convivem na mesma pagina com
// TTLs diferentes. Estas funcoes lancam em falha; os fetchers de alto nivel
// (repositorio.ts/contribuicoes.ts) capturam e degradam.

const REST_BASE = "https://api.github.com";
const GRAPHQL_URL = "https://api.github.com/graphql";

function cabecalhos(token: string | undefined): Record<string, string> {
  const base: Record<string, string> = {
    Accept: "application/vnd.github+json",
    "X-GitHub-Api-Version": "2022-11-28",
  };
  if (token) {
    base.Authorization = `Bearer ${token}`;
  }
  return base;
}

export async function githubRest<T>(
  caminho: string,
  revalidate: number,
  token: string | undefined,
): Promise<T> {
  const resposta = await fetch(`${REST_BASE}${caminho}`, {
    headers: cabecalhos(token),
    next: { revalidate },
  });
  if (!resposta.ok) {
    throw new Error(`GitHub REST ${caminho} respondeu ${resposta.status}`);
  }
  return (await resposta.json()) as T;
}

export async function githubGraphQL<T>(
  consulta: string,
  variaveis: Record<string, unknown>,
  revalidate: number,
  token: string | undefined,
): Promise<T> {
  const resposta = await fetch(GRAPHQL_URL, {
    method: "POST",
    headers: { ...cabecalhos(token), "Content-Type": "application/json" },
    body: JSON.stringify({ query: consulta, variables: variaveis }),
    next: { revalidate },
  });
  if (!resposta.ok) {
    throw new Error(`GitHub GraphQL respondeu ${resposta.status}`);
  }
  const corpo = (await resposta.json()) as { data?: T; errors?: unknown[] };
  if (corpo.errors && corpo.errors.length > 0) {
    throw new Error("GitHub GraphQL retornou errors");
  }
  if (corpo.data === undefined) {
    throw new Error("GitHub GraphQL respondeu sem data");
  }
  return corpo.data;
}
