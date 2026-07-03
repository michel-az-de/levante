"use client";

import { useState } from "react";

/**
 * Inscricao na newsletter (double opt-in). Envia pelo BFF publico (/api/publico),
 * mantendo a invariante de que o browser nunca fala com a API .NET direto. Anti-spam:
 * honeypot escondido (bots preenchem). A resposta e sempre a mesma — nao revela se o
 * e-mail ja existe (privacidade/LGPD). A inscricao so vale apos confirmar por e-mail.
 */
export function FormInscricaoNewsletter() {
  const [email, setEmail] = useState("");
  const [armadilha, setArmadilha] = useState("");
  const [enviando, setEnviando] = useState(false);
  const [erro, setErro] = useState<string | null>(null);
  const [enviado, setEnviado] = useState(false);

  async function enviar(evento: React.FormEvent) {
    evento.preventDefault();
    setErro(null);
    setEnviando(true);
    try {
      const resposta = await fetch("/api/publico/newsletter", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ email, armadilha }),
      });

      if (resposta.ok) {
        setEnviado(true);
        setEmail("");
      } else if (resposta.status === 400) {
        setErro("Verifique o e-mail e tente novamente.");
      } else {
        setErro("Nao foi possivel inscrever. Tente novamente.");
      }
    } catch {
      setErro("Falha de conexao. Tente novamente.");
    } finally {
      setEnviando(false);
    }
  }

  if (enviado) {
    return (
      <p className="rounded-md bg-green-50 px-3 py-2 text-sm text-green-800 dark:bg-green-950 dark:text-green-200">
        Quase la! Enviamos um e-mail de confirmacao. Clique no link para concluir a inscricao.
      </p>
    );
  }

  return (
    <form onSubmit={enviar} className="flex flex-col gap-3">
      <input
        type="email"
        required
        value={email}
        onChange={(e) => setEmail(e.target.value)}
        placeholder="seu@email.com"
        aria-label="Seu e-mail"
        className="rounded-md border border-neutral-300 px-3 py-2 dark:border-neutral-700 dark:bg-neutral-900"
      />

      {/* Honeypot: escondido de humanos; bots preenchem e sao descartados no servidor. */}
      <div aria-hidden="true" className="pointer-events-none absolute -left-[9999px] h-0 w-0 overflow-hidden">
        <label>
          Nao preencha este campo
          <input
            type="text"
            tabIndex={-1}
            autoComplete="off"
            value={armadilha}
            onChange={(e) => setArmadilha(e.target.value)}
          />
        </label>
      </div>

      {erro ? <p className="text-sm text-red-600">{erro}</p> : null}

      <button
        type="submit"
        disabled={enviando}
        className="self-start rounded-md bg-neutral-900 px-4 py-2 text-sm text-white transition hover:bg-neutral-700 disabled:opacity-50 dark:bg-white dark:text-neutral-900 dark:hover:bg-neutral-200"
      >
        {enviando ? "Enviando..." : "Inscrever"}
      </button>
    </form>
  );
}
