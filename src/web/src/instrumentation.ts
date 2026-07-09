// Fail-fast de configuracao no BOOT do servidor (nao no build): register() roda uma vez
// quando o processo Next sobe. Sem SITE_URL em producao, canonical/sitemap/RSS/OG sairiam
// apontando para localhost e seriam indexados assim — melhor derrubar o container do que
// servir URLs erradas. O `next build` do CI nao chama register(), entao nao quebra o pipeline.
export function register(): void {
  // Roda tambem no runtime edge; a validacao so faz sentido no servidor Node (standalone).
  if (process.env.NEXT_RUNTIME !== "nodejs") {
    return;
  }

  if (process.env.NODE_ENV === "production" && !process.env.SITE_URL) {
    throw new Error(
      "[levante] SITE_URL obrigatorio em producao (canonical/sitemap/RSS/OG/JSON-LD). " +
        "Defina SITE_URL no ambiente do container.",
    );
  }
}
