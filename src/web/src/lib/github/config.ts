// Configuracao da integracao GitHub (fatia E3, ADR 0006). Le do ambiente em
// tempo de chamada (nao no import) para ser testavel e para nao derrubar o boot
// quando o token falta — sem token, os fetchers apenas degradam.
//
// TODO(GAP-K): as contas abaixo sao placeholders configuraveis via env ate o
// Felipe confirmar se `felipeazevedoit` (perfil pessoal) e `michel-az-de` (dono
// do repo/CI) sao a mesma pessoa, e os owners reais de oracle-pack/easystock/hiram.

export type ConfigGithub = {
  /** PAT de leitura publica (server-only). Ausente = integracao degrada. */
  token: string | undefined;
  /** Perfil pessoal: heatmap de contribuicoes e "dono exibido" dos cards. */
  perfil: string;
  /** Conta tecnica/organizacao onde o codigo mora e o CI publica. */
  organizacao: string;
  /** "owner/repo" do card unico do produto na landing /levante. */
  repoLevante: string;
  /** Lista "owner/repo" dos cards do bento do site pessoal. */
  reposVitrine: string[];
};

const PADRAO_VITRINE = [
  "michel-az-de/levante",
  "felipeazevedoit/oracle-pack",
  "felipeazevedoit/easystock",
  "felipeazevedoit/hiram",
];

export function lerConfigGithub(): ConfigGithub {
  const env = process.env;
  const vitrine = env.GITHUB_SHOWCASE_REPOS?.split(",")
    .map((item) => item.trim())
    .filter(Boolean);

  return {
    token: env.GITHUB_API_TOKEN || undefined,
    perfil: env.GITHUB_PROFILE_ACCOUNT ?? "felipeazevedoit",
    organizacao: env.GITHUB_ORG_ACCOUNT ?? "michel-az-de",
    repoLevante: env.GITHUB_LEVANTE_REPO ?? "michel-az-de/levante",
    reposVitrine: vitrine && vitrine.length > 0 ? vitrine : PADRAO_VITRINE,
  };
}
