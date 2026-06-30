"use client";

import { useRouter } from "next/navigation";
import { useEffect, useState } from "react";
import { apiAdmin, limparToken, obterToken } from "@/lib/auth";

/**
 * Guarda das telas de admin (client): exige token e o valida no servidor
 * (/auth/eu). A autorizacao real e sempre do servidor (JWT). O setState fica
 * dentro do callback assincrono (evita cascata sincrona no efeito). Retorna se
 * a tela pode renderizar.
 */
export function useGuardaAdmin(): boolean {
  const router = useRouter();
  const [autorizado, setAutorizado] = useState(false);

  useEffect(() => {
    if (!obterToken()) {
      router.replace("/admin/login");
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
          limparToken();
          router.replace("/admin/login");
          return;
        }
        setAutorizado(true);
      })
      .catch(() => {
        // Falha de rede (API fora/CORS): trata como nao autenticado em vez de
        // travar a tela em "Carregando..." para sempre.
        if (ativo) {
          limparToken();
          router.replace("/admin/login");
        }
      });

    return () => {
      ativo = false;
    };
  }, [router]);

  return autorizado;
}

/** Trata 401 do servidor (token expirado/invalido): limpa e manda ao login. */
export function tratarNaoAutorizado(status: number, router: { replace: (url: string) => void }): boolean {
  if (status === 401) {
    limparToken();
    router.replace("/admin/login");
    return true;
  }
  return false;
}
