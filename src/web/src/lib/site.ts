// Configuracao do site para SEO (canonical, sitemap, RSS, OG, JSON-LD).
// SITE_URL via env (GAP-A: dominio ainda em aberto). LinkedIn via env (TODO).

const linkedin = process.env.LINKEDIN_URL;

export const site = {
  url: (process.env.SITE_URL ?? "http://localhost:3000").replace(/\/$/, ""),
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
