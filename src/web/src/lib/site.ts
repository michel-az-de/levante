// Configuracao do site para SEO (canonical, sitemap, RSS, OG, JSON-LD).
// SITE_URL via env (GAP-A: dominio ainda em aberto). LinkedIn via env (TODO).

const linkedin = process.env.LINKEDIN_URL;
const siteUrlEnv = process.env.SITE_URL;

// SITE_URL e o input mais importante desta fatia: sem ele, canonical/sitemap/RSS/OG
// sairiam apontando para localhost e seriam indexados assim. NAO lancamos aqui
// (quebraria o `next build` do CI, que roda em producao sem a env); avisamos alto.
// TODO(infra): fail-fast definitivo no boot do container (start, nao build).
if (!siteUrlEnv && process.env.NODE_ENV === "production") {
  console.warn(
    "[levante] SITE_URL nao definido em producao: usando http://localhost:3000. " +
      "Canonical, sitemap, RSS e OpenGraph sairao com URL incorreta. Defina SITE_URL.",
  );
}

export const site = {
  url: (siteUrlEnv ?? "http://localhost:3000").replace(/\/$/, ""),
  nome: "Levante",
  autor: "Felipe Michel de Azevedo",
  jobTitle: "Arquiteto de solucoes e desenvolvedor senior .NET / full stack",
  descricao:
    "Plataforma pessoal e portfolio tecnico de Felipe Michel de Azevedo: blog tecnico, publicacoes e vitrine de projetos.",
  // sameAs: GitHub agora; LinkedIn entra quando LINKEDIN_URL for definido (TODO).
  sameAs: ["https://github.com/michel-az-de", ...(linkedin ? [linkedin] : [])],
  knowsAbout: [
    "Arquitetura de software",
    ".NET",
    "Domain-Driven Design",
    "Clean Architecture",
    "Azure",
  ],
} as const;
