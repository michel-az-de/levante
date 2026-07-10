// Alterna o tema claro/escuro do site no <html> e persiste em localStorage.
// Compartilhado pelo TemaToggle e pelo cmd-k. O script anti-FOUC (boot) le a
// mesma chave para restaurar antes do primeiro paint.

const ATRIBUTO = "data-theme";
const CHAVE_STORAGE = "levante:tema";

export function alternarTema(): void {
  const atual = document.documentElement.getAttribute(ATRIBUTO) === "light" ? "light" : "dark";
  const novo = atual === "light" ? "dark" : "light";
  document.documentElement.setAttribute(ATRIBUTO, novo);
  try {
    localStorage.setItem(CHAVE_STORAGE, novo);
  } catch {
    // localStorage indisponivel (modo privado): a escolha vale so nesta sessao.
  }
}
