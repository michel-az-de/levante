"use client";

import {
  createContext,
  useCallback,
  useContext,
  useSyncExternalStore,
  type ReactNode,
} from "react";
import { type ChaveTexto, type Idioma, textos } from "./textos";

const ATRIBUTO = "data-idioma";
const CHAVE_STORAGE = "levante:idioma";

// A fonte da verdade do idioma e o atributo data-idioma no <html> (ajustado pelo
// script anti-FOUC antes da hidratacao). useSyncExternalStore le esse estado
// externo sem setState-em-efeito e sem mismatch de hidratacao: no servidor le
// "pt"; no cliente, o valor real. As unicas escritoras sao definir/alternar,
// que notificam os ouvintes de forma sincrona.
const ouvintes = new Set<() => void>();

function subscrever(callback: () => void): () => void {
  ouvintes.add(callback);
  return () => {
    ouvintes.delete(callback);
  };
}

function lerDoDom(): Idioma {
  return document.documentElement.getAttribute(ATRIBUTO) === "en" ? "en" : "pt";
}

function lerNoServidor(): Idioma {
  return "pt";
}

function escrever(idioma: Idioma): void {
  document.documentElement.setAttribute(ATRIBUTO, idioma);
  try {
    localStorage.setItem(CHAVE_STORAGE, idioma);
  } catch {
    // localStorage indisponivel (modo privado): a escolha vale so nesta sessao.
  }
  for (const ouvinte of ouvintes) {
    ouvinte();
  }
}

type IdiomaContexto = {
  idioma: Idioma;
  alternar: () => void;
  definir: (idioma: Idioma) => void;
  t: (chave: ChaveTexto) => string;
};

const Contexto = createContext<IdiomaContexto | null>(null);

/**
 * Provedor do idioma de chrome (ADR 0005). Le o atributo data-idioma do <html>
 * via useSyncExternalStore — no servidor sempre "pt", no cliente o valor real ja
 * ajustado pelo script anti-FOUC, sem mismatch de hidratacao. So o chrome usa
 * isto; o conteudo de artigo continua so em PT.
 */
export function IdiomaProvider({ children }: { children: ReactNode }) {
  const idioma = useSyncExternalStore(subscrever, lerDoDom, lerNoServidor);

  const definir = useCallback((novo: Idioma) => {
    escrever(novo);
  }, []);

  const alternar = useCallback(() => {
    escrever(lerDoDom() === "en" ? "pt" : "en");
  }, []);

  const t = useCallback((chave: ChaveTexto) => textos[chave][idioma], [idioma]);

  return (
    <Contexto.Provider value={{ idioma, alternar, definir, t }}>{children}</Contexto.Provider>
  );
}

export function useIdioma(): IdiomaContexto {
  const contexto = useContext(Contexto);
  if (contexto === null) {
    throw new Error("useIdioma precisa de um IdiomaProvider acima na arvore.");
  }
  return contexto;
}
