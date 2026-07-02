"use client";

import { useEffect, useState } from "react";

/** Estado das reacoes retornado pelo BFF publico (/api/publico). */
type ReacoesView = {
  curtir: number;
  amei: number;
  relevante: number;
  minhas: string[];
};

const TIPOS: readonly { tipo: string; rotulo: string; chave: keyof ReacoesView }[] = [
  { tipo: "Curtir", rotulo: "Curtir", chave: "curtir" },
  { tipo: "Amei", rotulo: "Amei", chave: "amei" },
  { tipo: "Relevante", rotulo: "Relevante", chave: "relevante" },
];

function contagem(reacoes: ReacoesView | null, chave: keyof ReacoesView): number {
  const valor = reacoes?.[chave];
  return typeof valor === "number" ? valor : 0;
}

/**
 * Botoes de reacao anonima a um artigo. Chama o BFF publico (/api/publico), que
 * cuida do cookie de visitante. Reacoes sao enfeite: falhas nao quebram a pagina.
 */
export function ReacoesArtigo({ artigoId }: { artigoId: string }) {
  const [reacoes, setReacoes] = useState<ReacoesView | null>(null);
  const [ocupado, setOcupado] = useState(false);
  const [erro, setErro] = useState<string | null>(null);

  useEffect(() => {
    let ativo = true;
    // setState dentro do callback assincrono (nao sincrono no efeito).
    fetch(`/api/publico/artigos/${artigoId}/reacoes`, { cache: "no-store" })
      .then(async (resposta) => {
        if (ativo && resposta.ok) {
          setReacoes((await resposta.json()) as ReacoesView);
        }
      })
      .catch(() => {
        // Silencioso: reacoes sao enfeite; nao travar a leitura do artigo.
      });

    return () => {
      ativo = false;
    };
  }, [artigoId]);

  async function alternar(tipo: string) {
    if (ocupado) {
      return;
    }
    setOcupado(true);
    setErro(null);

    const jaReagi = reacoes?.minhas.includes(tipo) ?? false;
    try {
      const resposta = jaReagi
        ? await fetch(`/api/publico/artigos/${artigoId}/reacoes/${tipo}`, { method: "DELETE" })
        : await fetch(`/api/publico/artigos/${artigoId}/reacoes`, {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ tipo }),
          });

      if (resposta.ok) {
        setReacoes((await resposta.json()) as ReacoesView);
      } else {
        setErro("Nao foi possivel registrar sua reacao. Tente novamente.");
      }
    } catch {
      setErro("Falha de conexao. Tente novamente.");
    } finally {
      setOcupado(false);
    }
  }

  return (
    <section
      aria-label="Reacoes"
      className="flex flex-col gap-2 border-t border-neutral-200 pt-4 dark:border-neutral-800"
    >
      <div className="flex flex-wrap gap-2">
        {TIPOS.map(({ tipo, rotulo, chave }) => {
          const ativo = reacoes?.minhas.includes(tipo) ?? false;
          return (
            <button
              key={tipo}
              type="button"
              disabled={ocupado}
              aria-pressed={ativo}
              onClick={() => void alternar(tipo)}
              className={`rounded-full border px-3 py-1 text-sm transition disabled:opacity-50 ${
                ativo
                  ? "border-neutral-900 bg-neutral-900 text-white dark:border-white dark:bg-white dark:text-neutral-900"
                  : "border-neutral-300 hover:bg-neutral-100 dark:border-neutral-700 dark:hover:bg-neutral-800"
              }`}
            >
              {rotulo} <span className="tabular-nums">{contagem(reacoes, chave)}</span>
            </button>
          );
        })}
      </div>
      {erro ? <p className="text-sm text-red-600">{erro}</p> : null}
    </section>
  );
}
