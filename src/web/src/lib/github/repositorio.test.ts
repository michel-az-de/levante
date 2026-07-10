import { afterEach, describe, expect, it, vi } from "vitest";
import {
  buscarCommitsRecentesGithub,
  buscarIssuesAbertasGithub,
  buscarRepositorioGithub,
} from "@/lib/github/repositorio";

describe("repositorio GitHub (REST)", () => {
  afterEach(() => {
    vi.unstubAllGlobals();
  });

  it("normaliza os metadados do repositorio", async () => {
    const bruto = {
      full_name: "michel-az-de/levante",
      description: "engine headless",
      language: "C#",
      license: { spdx_id: "Apache-2.0" },
      stargazers_count: 48,
      forks_count: 6,
      pushed_at: "2026-06-27T15:20:00Z",
      html_url: "https://github.com/michel-az-de/levante",
    };
    vi.stubGlobal("fetch", vi.fn().mockResolvedValue(new Response(JSON.stringify(bruto), { status: 200 })));

    const repo = await buscarRepositorioGithub("michel-az-de/levante");

    expect(repo?.estrelas).toBe(48);
    expect(repo?.licenca).toBe("Apache-2.0");
    expect(repo?.linguagem).toBe("C#");
    expect(repo?.nomeCompleto).toBe("michel-az-de/levante");
  });

  it("retorna null quando a API falha (degradacao honesta)", async () => {
    vi.stubGlobal("fetch", vi.fn().mockResolvedValue(new Response("erro", { status: 500 })));
    expect(await buscarRepositorioGithub("x/y")).toBeNull();
  });

  it("filtra pull requests da lista de issues e respeita o limite", async () => {
    const brutas = [
      {
        number: 1,
        title: "issue de verdade",
        labels: [{ name: "bug" }],
        user: { login: "a" },
        created_at: "2026-01-01T00:00:00Z",
        html_url: "u1",
      },
      {
        number: 2,
        title: "isto e um PR",
        labels: [],
        user: { login: "a" },
        created_at: "2026-01-02T00:00:00Z",
        html_url: "u2",
        pull_request: { url: "..." },
      },
    ];
    vi.stubGlobal("fetch", vi.fn().mockResolvedValue(new Response(JSON.stringify(brutas), { status: 200 })));

    const issues = await buscarIssuesAbertasGithub("x/y", 5);

    expect(issues).toHaveLength(1);
    expect(issues[0].numero).toBe(1);
    expect(issues[0].labels).toEqual(["bug"]);
  });

  it("encurta o sha e pega so a primeira linha da mensagem do commit", async () => {
    const brutos = [
      {
        sha: "a1b2c3d4e5f6",
        commit: { message: "feat: x\n\ncorpo longo", author: { name: "Felipe", date: "2026-01-01T00:00:00Z" } },
        author: { login: "f" },
        html_url: "u",
      },
    ];
    vi.stubGlobal("fetch", vi.fn().mockResolvedValue(new Response(JSON.stringify(brutos), { status: 200 })));

    const commits = await buscarCommitsRecentesGithub("x/y");

    expect(commits[0].sha).toBe("a1b2c3d");
    expect(commits[0].mensagem).toBe("feat: x");
    expect(commits[0].autor).toBe("Felipe");
  });

  it("retorna [] quando a listagem falha", async () => {
    vi.stubGlobal("fetch", vi.fn().mockResolvedValue(new Response("erro", { status: 502 })));
    expect(await buscarIssuesAbertasGithub("x/y")).toEqual([]);
    expect(await buscarCommitsRecentesGithub("x/y")).toEqual([]);
  });
});
