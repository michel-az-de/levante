"use client";

import { useRouter } from "next/navigation";
import { useCallback, useEffect, useMemo, useRef, useState } from "react";
import { filtrarComandos } from "@/lib/cmdk";
import { useIdioma } from "@/lib/i18n/IdiomaProvider";
import { alternarTema } from "@/lib/tema";

/** Evento que o botao ⌘K do Header dispara para abrir a paleta. */
export const EVENTO_ABRIR_CMDK = "levante:abrir-cmdk";

type Comando = { id: string; pt: string; en: string; icone: string; acao: () => void };

function irParaSecao(id: string): void {
  if (window.location.pathname === "/") {
    document.getElementById(id)?.scrollIntoView({ behavior: "smooth" });
  } else {
    window.location.assign(`/#${id}`);
  }
}

/** Paleta de comandos (⌘K / Ctrl+K): navega, alterna tema, abre o GitHub. */
export function CmdK() {
  const router = useRouter();
  const { idioma } = useIdioma();
  const dialogo = useRef<HTMLDialogElement>(null);
  const [busca, setBusca] = useState("");
  const [indice, setIndice] = useState(0);

  const comandos = useMemo<Comando[]>(
    () => [
      { id: "consultoria", pt: "Consultoria", en: "Consulting", icone: "§", acao: () => irParaSecao("consultoria") },
      { id: "capacidades", pt: "O que eu faço", en: "What I do", icone: "§", acao: () => irParaSecao("capacidades") },
      { id: "github", pt: "Código", en: "Code", icone: "§", acao: () => irParaSecao("github") },
      { id: "artigos", pt: "Artigos", en: "Articles", icone: "§", acao: () => router.push("/artigos") },
      { id: "experiencia", pt: "Experiência", en: "Experience", icone: "§", acao: () => irParaSecao("experiencia") },
      { id: "contato", pt: "Contato", en: "Contact", icone: "@", acao: () => irParaSecao("contato") },
      { id: "newsletter", pt: "Assinar a newsletter", en: "Subscribe", icone: "→", acao: () => router.push("/newsletter") },
      { id: "tema", pt: "Alternar tema", en: "Toggle theme", icone: "◐", acao: alternarTema },
      { id: "gh", pt: "GitHub", en: "GitHub", icone: "↗", acao: () => window.open("https://github.com/felipeazevedoit", "_blank", "noopener") },
    ],
    [router],
  );

  const filtrados = filtrarComandos(comandos, busca);
  const ativo = Math.min(indice, Math.max(0, filtrados.length - 1));

  const abrir = useCallback(() => {
    setBusca("");
    setIndice(0);
    dialogo.current?.showModal();
  }, []);
  const fechar = useCallback(() => dialogo.current?.close(), []);
  const executar = useCallback(
    (comando: Comando) => {
      fechar();
      comando.acao();
    },
    [fechar],
  );

  useEffect(() => {
    function aoTeclar(evento: KeyboardEvent) {
      if ((evento.metaKey || evento.ctrlKey) && evento.key.toLowerCase() === "k") {
        evento.preventDefault();
        if (dialogo.current?.open) {
          fechar();
        } else {
          abrir();
        }
      }
    }
    window.addEventListener("keydown", aoTeclar);
    window.addEventListener(EVENTO_ABRIR_CMDK, abrir);
    return () => {
      window.removeEventListener("keydown", aoTeclar);
      window.removeEventListener(EVENTO_ABRIR_CMDK, abrir);
    };
  }, [abrir, fechar]);

  function navegar(evento: React.KeyboardEvent<HTMLInputElement>) {
    if (evento.key === "ArrowDown") {
      evento.preventDefault();
      setIndice((i) => Math.min(filtrados.length - 1, i + 1));
    } else if (evento.key === "ArrowUp") {
      evento.preventDefault();
      setIndice((i) => Math.max(0, i - 1));
    } else if (evento.key === "Enter") {
      evento.preventDefault();
      const comando = filtrados[ativo];
      if (comando) {
        executar(comando);
      }
    }
  }

  return (
    <dialog
      ref={dialogo}
      aria-label="Paleta de comandos"
      className="m-auto w-[min(520px,92vw)] bg-site-bg1 text-site-fg backdrop:bg-black/50 backdrop:backdrop-blur-sm"
    >
      <div className="border border-site-line2">
        <input
          autoFocus
          value={busca}
          onChange={(evento) => {
            setBusca(evento.target.value);
            setIndice(0);
          }}
          onKeyDown={navegar}
          placeholder={idioma === "en" ? "Search..." : "Buscar..."}
          className="w-full border-b border-site-line bg-transparent px-[18px] py-4 text-[15px] text-site-fg outline-none"
        />
        <div className="max-h-[320px] overflow-y-auto p-[7px]">
          {filtrados.length === 0 ? (
            <div className="px-3.5 py-3 text-sm text-site-faint">—</div>
          ) : (
            filtrados.map((comando, i) => (
              <button
                key={comando.id}
                type="button"
                onClick={() => executar(comando)}
                onMouseEnter={() => setIndice(i)}
                className={`flex w-full items-center gap-3 px-3.5 py-3 text-left text-sm transition-colors ${
                  i === ativo ? "bg-site-acc/12 text-site-fg" : "text-site-fg2"
                }`}
              >
                <span
                  className={`w-4 text-center font-site-mono text-xs ${
                    i === ativo ? "text-site-acc" : "text-site-faint"
                  }`}
                >
                  {comando.icone}
                </span>
                {idioma === "en" ? comando.en : comando.pt}
              </button>
            ))
          )}
        </div>
      </div>
    </dialog>
  );
}
