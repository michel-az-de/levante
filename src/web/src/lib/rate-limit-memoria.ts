// Rate limit in-memory, janela fixa, por chave (ex.: IP). Bounded para nao vazar
// memoria. Consistente com o rate limit in-memory aceito no MVP (por replica, reseta
// no restart do container). `agora` e injetado para o limitador ser puro/testavel.

type Registro = { count: number; reset: number };

/** Teto de chaves distintas guardadas; ao estourar, o mapa e descartado (bound cru). */
const MAX_CHAVES = 10_000;

/**
 * Cria um limitador: permite ate `limite` chamadas por `janelaMs` por chave.
 * Retorna `true` se a chamada e permitida, `false` se excedeu a janela.
 */
export function criarLimitador(limite: number, janelaMs: number) {
  const registros = new Map<string, Registro>();

  return function permitir(chave: string, agora: number): boolean {
    const atual = registros.get(chave);
    if (!atual || agora >= atual.reset) {
      if (registros.size >= MAX_CHAVES) {
        registros.clear();
      }
      registros.set(chave, { count: 1, reset: agora + janelaMs });
      return true;
    }
    if (atual.count >= limite) {
      return false;
    }
    atual.count += 1;
    return true;
  };
}
