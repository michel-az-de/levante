"use client";

import { useRef, useState } from "react";
import { Markdown } from "@/components/Markdown";

export interface ArtigoFormValores {
  titulo: string;
  slug: string;
  resumo: string;
  conteudo: string;
}

/**
 * Editor de artigo reutilizado por "novo" e "editar". Campos controlados +
 * textarea de conteudo com preview ao vivo (mesmo renderizador do publico) e
 * uma toolbar minima de markdown. onSubmit devolve uma mensagem de erro (ou null).
 */
export function ArtigoEditor({
  inicial,
  textoAcao,
  onSubmit,
}: {
  inicial: ArtigoFormValores;
  textoAcao: string;
  onSubmit: (valores: ArtigoFormValores) => Promise<string | null>;
}) {
  const [titulo, setTitulo] = useState(inicial.titulo);
  const [slug, setSlug] = useState(inicial.slug);
  const [resumo, setResumo] = useState(inicial.resumo);
  const [conteudo, setConteudo] = useState(inicial.conteudo);
  const [erro, setErro] = useState<string | null>(null);
  const [enviando, setEnviando] = useState(false);
  const conteudoRef = useRef<HTMLTextAreaElement>(null);

  function envolverSelecao(prefixo: string, sufixo: string, exemplo: string) {
    const el = conteudoRef.current;
    if (!el) {
      return;
    }
    const inicio = el.selectionStart;
    const fim = el.selectionEnd;
    const selecionado = conteudo.slice(inicio, fim) || exemplo;
    const novo = conteudo.slice(0, inicio) + prefixo + selecionado + sufixo + conteudo.slice(fim);
    setConteudo(novo);
    requestAnimationFrame(() => {
      el.focus();
      const pos = inicio + prefixo.length;
      el.setSelectionRange(pos, pos + selecionado.length);
    });
  }

  function prefixarLinha(prefixo: string) {
    const el = conteudoRef.current;
    if (!el) {
      return;
    }
    const inicio = el.selectionStart;
    const inicioLinha = conteudo.lastIndexOf("\n", inicio - 1) + 1;
    const novo = conteudo.slice(0, inicioLinha) + prefixo + conteudo.slice(inicioLinha);
    setConteudo(novo);
    requestAnimationFrame(() => {
      el.focus();
      const pos = inicio + prefixo.length;
      el.setSelectionRange(pos, pos);
    });
  }

  async function enviar(evento: React.FormEvent) {
    evento.preventDefault();
    setErro(null);
    setEnviando(true);
    const mensagem = await onSubmit({ titulo, slug, resumo, conteudo });
    setEnviando(false);
    if (mensagem) {
      setErro(mensagem);
    }
  }

  const botaoToolbar =
    "rounded border border-neutral-300 px-2 py-1 text-xs transition hover:bg-neutral-100 dark:border-neutral-700 dark:hover:bg-neutral-800";

  return (
    <form onSubmit={enviar} className="flex flex-col gap-5">
      <label className="flex flex-col gap-1 text-sm">
        Titulo
        <input
          type="text"
          required
          value={titulo}
          onChange={(e) => setTitulo(e.target.value)}
          className="rounded-md border border-neutral-300 px-3 py-2 dark:border-neutral-700 dark:bg-neutral-900"
        />
      </label>

      <label className="flex flex-col gap-1 text-sm">
        Slug (kebab-case)
        <input
          type="text"
          required
          value={slug}
          onChange={(e) => setSlug(e.target.value)}
          placeholder="clean-architecture-na-pratica"
          className="rounded-md border border-neutral-300 px-3 py-2 font-mono dark:border-neutral-700 dark:bg-neutral-900"
        />
      </label>

      <label className="flex flex-col gap-1 text-sm">
        Resumo (ate 280 caracteres)
        <textarea
          required
          maxLength={280}
          rows={2}
          value={resumo}
          onChange={(e) => setResumo(e.target.value)}
          className="rounded-md border border-neutral-300 px-3 py-2 dark:border-neutral-700 dark:bg-neutral-900"
        />
        <span className="text-xs text-neutral-500">{resumo.length}/280</span>
      </label>

      <div className="flex flex-col gap-1 text-sm">
        Conteudo (markdown)
        <div className="flex flex-wrap gap-1">
          <button type="button" className={botaoToolbar} onClick={() => envolverSelecao("**", "**", "negrito")}>
            Negrito
          </button>
          <button type="button" className={botaoToolbar} onClick={() => envolverSelecao("_", "_", "italico")}>
            Italico
          </button>
          <button type="button" className={botaoToolbar} onClick={() => prefixarLinha("## ")}>
            Titulo
          </button>
          <button type="button" className={botaoToolbar} onClick={() => prefixarLinha("- ")}>
            Lista
          </button>
          <button type="button" className={botaoToolbar} onClick={() => envolverSelecao("[", "](https://)", "texto")}>
            Link
          </button>
          <button type="button" className={botaoToolbar} onClick={() => envolverSelecao("`", "`", "codigo")}>
            Codigo
          </button>
        </div>
        <div className="grid gap-4 md:grid-cols-2">
          <textarea
            ref={conteudoRef}
            required
            rows={18}
            value={conteudo}
            onChange={(e) => setConteudo(e.target.value)}
            className="rounded-md border border-neutral-300 px-3 py-2 font-mono text-sm dark:border-neutral-700 dark:bg-neutral-900"
          />
          <div className="min-h-[8rem] overflow-auto rounded-md border border-neutral-200 px-4 py-3 dark:border-neutral-800">
            {conteudo.trim() ? (
              <Markdown>{conteudo}</Markdown>
            ) : (
              <p className="text-sm text-neutral-400">Preview do conteudo aparece aqui.</p>
            )}
          </div>
        </div>
      </div>

      {erro ? <p className="text-sm text-red-600">{erro}</p> : null}

      <button
        type="submit"
        disabled={enviando}
        className="self-start rounded-md bg-neutral-900 px-4 py-2 text-white transition hover:bg-neutral-700 disabled:opacity-50 dark:bg-white dark:text-neutral-900 dark:hover:bg-neutral-200"
      >
        {enviando ? "Salvando..." : textoAcao}
      </button>
    </form>
  );
}
