import { afterEach, describe, expect, it, vi } from "vitest";
import { buscarContribuicoesGithub } from "@/lib/github/contribuicoes";

describe("contribuicoes GitHub (GraphQL)", () => {
  afterEach(() => {
    vi.unstubAllGlobals();
    vi.unstubAllEnvs();
  });

  it("retorna null sem token (GraphQL exige auth) e nem chama a rede", async () => {
    vi.stubEnv("GITHUB_API_TOKEN", "");
    const fetchMock = vi.fn();
    vi.stubGlobal("fetch", fetchMock);

    expect(await buscarContribuicoesGithub("felipeazevedoit")).toBeNull();
    expect(fetchMock).not.toHaveBeenCalled();
  });

  it("mapeia os niveis de contribuicao quando ha token", async () => {
    vi.stubEnv("GITHUB_API_TOKEN", "tok");
    const dados = {
      data: {
        user: {
          contributionsCollection: {
            contributionCalendar: {
              totalContributions: 3,
              weeks: [
                {
                  contributionDays: [
                    { date: "2026-01-01", contributionCount: 0, contributionLevel: "NONE" },
                    { date: "2026-01-02", contributionCount: 5, contributionLevel: "FOURTH_QUARTILE" },
                  ],
                },
              ],
            },
          },
        },
      },
    };
    vi.stubGlobal("fetch", vi.fn().mockResolvedValue(new Response(JSON.stringify(dados), { status: 200 })));

    const calendario = await buscarContribuicoesGithub("felipeazevedoit");

    expect(calendario?.total).toBe(3);
    expect(calendario?.semanas[0][0].nivel).toBe(0);
    expect(calendario?.semanas[0][1].nivel).toBe(4);
  });

  it("retorna null quando o GraphQL falha", async () => {
    vi.stubEnv("GITHUB_API_TOKEN", "tok");
    vi.stubGlobal("fetch", vi.fn().mockResolvedValue(new Response("erro", { status: 500 })));

    expect(await buscarContribuicoesGithub("felipeazevedoit")).toBeNull();
  });
});
