// Fetcher GraphQL do calendario de contribuicoes do GitHub (fatia E3, ADR 0006).
// A API REST nao expoe o calendario; o GraphQL v4 exige autenticacao, entao sem
// token a integracao degrada para null (o heatmap mostra estado neutro). TTL de
// 24h (contribuicoes mudam devagar e a query e mais cara).

import type {
  CalendarioContribuicoes,
  DiaContribuicao,
  NivelContribuicao,
} from "@/types/github";
import { githubGraphQL } from "./client";
import { lerConfigGithub } from "./config";

const TTL = 86400;

const CONSULTA = `query($usuario: String!) {
  user(login: $usuario) {
    contributionsCollection {
      contributionCalendar {
        totalContributions
        weeks {
          contributionDays { date contributionCount contributionLevel }
        }
      }
    }
  }
}`;

const NIVEIS: Record<string, NivelContribuicao> = {
  NONE: 0,
  FIRST_QUARTILE: 1,
  SECOND_QUARTILE: 2,
  THIRD_QUARTILE: 3,
  FOURTH_QUARTILE: 4,
};

type RespostaBruta = {
  user: {
    contributionsCollection: {
      contributionCalendar: {
        totalContributions: number;
        weeks: {
          contributionDays: {
            date: string;
            contributionCount: number;
            contributionLevel: string;
          }[];
        }[];
      };
    };
  } | null;
};

export async function buscarContribuicoesGithub(
  usuario: string,
): Promise<CalendarioContribuicoes | null> {
  const { token } = lerConfigGithub();
  if (!token) {
    return null;
  }
  try {
    const dados = await githubGraphQL<RespostaBruta>(CONSULTA, { usuario }, TTL, token);
    const calendario = dados.user?.contributionsCollection.contributionCalendar;
    if (!calendario) {
      return null;
    }
    const semanas: DiaContribuicao[][] = calendario.weeks.map((semana) =>
      semana.contributionDays.map((dia) => ({
        data: dia.date,
        total: dia.contributionCount,
        nivel: NIVEIS[dia.contributionLevel] ?? 0,
      })),
    );
    return { total: calendario.totalContributions, semanas };
  } catch {
    return null;
  }
}
