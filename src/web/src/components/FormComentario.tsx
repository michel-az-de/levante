"use client";

import { useState } from "react";

/**
 * Formulario de comentario anonimo (nome + texto; sem e-mail). Envia pelo BFF
 * publico (/api/publico). Anti-spam: campo honeypot escondido (bots preenchem).
 * O comentario nasce Pendente e so aparece apos moderacao.
 */
export function FormComentario({ artigoId, artigoSlug }: { artigoId: string; artigoSlug: string }) {
  const [autor, setAutor] = useState("");
  const [texto, setTexto] = useState("");
  const [armadilha, setArmadilha] = useState("");
  const [enviando, setEnviando] = useState(false);
  const [erro, setErro] = useState<string | null>(null);
  const [enviado, setEnviado] = useState(false);

  async function enviar(evento: React.FormEvent) {
    evento.preventDefault();
    setErro(null);
    setEnviando(true);
    try {
      const resposta = await fetch(`/api/publico/artigos/${artigoId}/comentarios`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ artigoSlug, autor, texto, armadilha }),
      });

      if (resposta.ok) {
        setEnviado(true);
        setAutor("");
        setTexto("");
      } else if (resposta.status === 400) {
        setErro("Verifique o nome e o comentario e tente novamente.");
      } else {
        setErro("Nao foi possivel enviar. Tente novamente.");
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
        Comentario enviado. Ele aparece assim que for aprovado na moderacao.
      </p>
    );
  }

  return (
    <form onSubmit={enviar} className="flex flex-col gap-3">
      <input
        type="text"
        required
        maxLength={60}
        value={autor}
        onChange={(e) => setAutor(e.target.value)}
        placeholder="Seu nome"
        aria-label="Seu nome"
        className="rounded-md border border-neutral-300 px-3 py-2 dark:border-neutral-700 dark:bg-neutral-900"
      />
      <textarea
        required
        maxLength={2000}
        rows={3}
        value={texto}
        onChange={(e) => setTexto(e.target.value)}
        placeholder="Escreva um comentario"
        aria-label="Comentario"
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
        {enviando ? "Enviando..." : "Comentar"}
      </button>
    </form>
  );
}
