"use client";

import { createContext, useContext, useEffect, useState } from "react";
import { useRouter, usePathname } from "next/navigation";
import { apiAdmin } from "@/lib/auth";

interface SessaoAdmin {
  email: string;
}

type EstadoAdmin =
  | { status: "carregando" }
  | { status: "autorizado"; sessao: SessaoAdmin }
  | { status: "naoAutorizado" };

const AdminAuthContext = createContext<EstadoAdmin>({ status: "carregando" });

const NAO_AUTORIZADO: EstadoAdmin = { status: "naoAutorizado" };

/**
 * Provedor de autenticacao compartilhado para todas as telas de admin. Valida a
 * sessao uma unica vez (nao a cada navegacao) e compartilha o resultado com os
 * filhos via contexto. Sessao expirada no meio do uso e tratada pelos fetches de
 * dados de cada tela (tratarNaoAutorizado em 401).
 */
export function AdminAuthProvider({ children }: { children: React.ReactNode }) {
  const router = useRouter();
  const pathname = usePathname();
  const [estado, setEstado] = useState<EstadoAdmin>({ status: "carregando" });
  const telaDeLogin = pathname === "/admin/login";

  useEffect(() => {
    // Login nao precisa de auth guard (o valor do contexto e derivado no render).
    if (telaDeLogin) {
      return;
    }

    // Sessao ja validada: nao repete /auth/eu a cada navegacao.
    if (estado.status === "autorizado") {
      return;
    }

    let ativo = true;
    apiAdmin
      .GET("/auth/eu")
      .then(({ data, error }) => {
        if (!ativo) {
          return;
        }
        if (error || !data) {
          setEstado(NAO_AUTORIZADO);
          router.replace("/admin/login");
          return;
        }
        setEstado({ status: "autorizado", sessao: { email: data.email } });
      })
      .catch(() => {
        if (ativo) {
          setEstado(NAO_AUTORIZADO);
          router.replace("/admin/login");
        }
      });

    return () => {
      ativo = false;
    };
  }, [router, telaDeLogin, estado.status]);

  return (
    <AdminAuthContext.Provider value={telaDeLogin ? NAO_AUTORIZADO : estado}>
      {children}
    </AdminAuthContext.Provider>
  );
}

/**
 * Hook para telas admin: retorna true quando a sessao esta validada.
 * Substitui useGuardaAdmin — nao faz nova chamada HTTP, usa o contexto
 * compartilhado do AdminAuthProvider.
 */
export function useAdminAuth(): boolean {
  const estado = useContext(AdminAuthContext);
  return estado.status === "autorizado";
}

/**
 * Hook para acessar os dados da sessao admin (email). So use depois de
 * verificar que a sessao esta autorizada (useAdminAuth === true).
 */
export function useSessaoAdmin(): SessaoAdmin | null {
  const estado = useContext(AdminAuthContext);
  return estado.status === "autorizado" ? estado.sessao : null;
}

/**
 * Trata 401 do servidor (sessao expirada/invalida): manda ao login.
 */
export function tratarNaoAutorizado(status: number, router: { replace: (url: string) => void }): boolean {
  if (status === 401) {
    router.replace("/admin/login");
    return true;
  }
  return false;
}
