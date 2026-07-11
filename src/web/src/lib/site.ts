// Configuracao do site para SEO (canonical, sitemap, RSS, OG, JSON-LD).
// SITE_URL via env (GAP-A: dominio ainda em aberto).
// O fail-fast de SITE_URL em producao vive em src/web/src/instrumentation.ts (boot do
// servidor, nao build); aqui so aplicamos o fallback localhost para dev/build.

const siteUrlEnv = process.env.SITE_URL;
// Identidade publica do Felipe (sameAs do Person). Configuravel via env; o
// default e o perfil pessoal exibido no site (TODO GAP-K: confirmar as contas).
const perfilGithub = process.env.GITHUB_PROFILE_ACCOUNT ?? "felipeazevedoit";
const linkedin = process.env.LINKEDIN_URL ?? "https://linkedin.com/in/felipe-azevedo-05493357";

export const site = {
  url: (siteUrlEnv ?? "http://localhost:3000").replace(/\/$/, ""),
  nome: "Levante",
  autor: "Felipe Michel de Azevedo",
  jobTitle: "Arquiteto de solucoes e desenvolvedor senior .NET / full stack",
  descricao:
    "Plataforma pessoal e portfolio tecnico de Felipe Michel de Azevedo: blog tecnico, publicacoes e vitrine de projetos.",
  // Perfis publicos do Person: GitHub pessoal + LinkedIn.
  sameAs: [`https://github.com/${perfilGithub}`, linkedin],
  knowsAbout: [
    "Arquitetura de software",
    ".NET",
    "Domain-Driven Design",
    "Clean Architecture",
    "Azure",
  ],
} as const;
