// Constantes do produto Levante (a engine open source) exibidas na landing
// /levante. repo/imagem espelham a config de CI; `dotnet` deve ser sincronizado
// manualmente com global.json.
export const produto = {
  repo: "michel-az-de/levante",
  urlRepo: "https://github.com/michel-az-de/levante",
  urlDocs: "https://github.com/michel-az-de/levante#readme",
  imagemDocker: "ghcr.io/michel-az-de/levante-api:latest",
  cloneUrl: "https://github.com/michel-az-de/levante.git",
  projetoApi: "src/api/host/Levante.Api",
  dotnet: ".NET 10",
  licenca: "Apache-2.0",
} as const;
