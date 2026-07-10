// Tipos internos normalizados da integracao GitHub (fatia E3, ADR 0006).
// Contrato de terceiro (nao e o dominio Levante): definidos a mao, NAO gerados
// do OpenAPI. Os fetchers em lib/github/* mapeiam a resposta bruta do GitHub
// para estes tipos, isolando o resto do app do formato da API externa.

export type RepositorioGithub = {
  /** "owner/repo". */
  nomeCompleto: string;
  descricao: string | null;
  linguagem: string | null;
  /** SPDX id da licenca (ex.: "Apache-2.0") ou null. */
  licenca: string | null;
  estrelas: number;
  forks: number;
  /** ISO 8601 do ultimo push, ou null. */
  atualizadoEm: string | null;
  url: string;
};

export type IssueGithub = {
  numero: number;
  titulo: string;
  labels: string[];
  autor: string | null;
  /** ISO 8601. */
  criadoEm: string;
  url: string;
};

export type CommitGithub = {
  /** SHA curto (7 chars). */
  sha: string;
  /** Primeira linha da mensagem. */
  mensagem: string;
  autor: string | null;
  /** ISO 8601 ou null. */
  data: string | null;
  url: string;
};

/** Intensidade de um dia no heatmap de contribuicoes (0 = nenhuma, 4 = maxima). */
export type NivelContribuicao = 0 | 1 | 2 | 3 | 4;

export type DiaContribuicao = {
  /** Data no formato "YYYY-MM-DD". */
  data: string;
  total: number;
  nivel: NivelContribuicao;
};

export type CalendarioContribuicoes = {
  total: number;
  /** Cada semana e uma coluna de ate 7 dias (mesma disposicao do heatmap do GitHub). */
  semanas: DiaContribuicao[][];
};
