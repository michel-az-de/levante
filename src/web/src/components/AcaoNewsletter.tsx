"use client";

import { useEffect, useState } from "react";

type Acao = "confirmar" | "cancelar";

const COPIA: Record<Acao, { carregando: string; ok: string }> = {
  confirmar: {
    carregando: "Confirmando sua inscricao...",
    ok: "Inscricao confirmada! Voce vai receber a newsletter.",
  },
  cancelar: {
    carregando: "Cancelando sua inscricao...",
    ok: "Inscricao cancelada. Voce nao vai mais receber a newsletter.",
  },
};

/**
 * Executa uma acao de token da newsletter (confirmar/cancelar) pelo BFF publico,
 * ao montar. O token vem do link enviado por e-mail; e a autorizacao da acao.
 */
export function AcaoNewsletter({ token, acao }: { token: string; acao: Acao }) {
  const [estado, setEstado] = useState<"carregando" | "ok" | "erro">("carregando");

  useEffect(() => {
    let ativo = true;

    async function executar() {
      if (!token) {
        if (ativo) {
          setEstado("erro");
        }
        return;
      }
      try {
        const resposta = await fetch(`/api/publico/newsletter/${acao}`, {
          method: "POST",
          headers: { "Content-Type": "application/json" },
          body: JSON.stringify({ token }),
        });
        if (ativo) {
          setEstado(resposta.ok ? "ok" : "erro");
        }
      } catch {
        if (ativo) {
          setEstado("erro");
        }
      }
    }

    executar();
    return () => {
      ativo = false;
    };
  }, [token, acao]);

  if (estado === "carregando") {
    return <p className="text-sm text-neutral-500">{COPIA[acao].carregando}</p>;
  }

  if (estado === "ok") {
    return (
      <p className="rounded-md bg-green-50 px-3 py-2 text-sm text-green-800 dark:bg-green-950 dark:text-green-200">
        {COPIA[acao].ok}
      </p>
    );
  }

  return (
    <p className="rounded-md bg-red-50 px-3 py-2 text-sm text-red-700 dark:bg-red-950 dark:text-red-200">
      Link invalido ou expirado. Se preciso, inscreva-se novamente.
    </p>
  );
}
