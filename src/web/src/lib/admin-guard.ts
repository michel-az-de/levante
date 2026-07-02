"use client";

import { useRouter } from "next/navigation";
import { useEffect, useState } from "react";
import { apiAdmin } from "@/lib/auth";

/**
 * Guarda das telas de admin (client): valida a sessao no servidor (/auth/eu via
 * BFF; o cookie httpOnly vai junto automaticamente). A autorizacao real e sempre
 * do servidor (JWT). O setState fica dentro do callback assincrono (evita cascata
 * sincrona no efeito). Retorna se a tela pode renderizar.
 */
export function useGuardaAdmin(): boolean {
  const router = useRouter();
  const [autorizado, setAutorizado] = useState(false);

  useEffect(() => {
    let ativo = true;
    apiAdmin
      .GET("/auth/eu")
      .then(({ data, error }) => {
        if (!ativo) {
          return;
        }
        if (error || !data) {
          router.replace("/admin/login");
          return;
        }
        setAutorizado(true);
      })
      .catch(() => {
        // Falha de rede (BFF/API fora): trata como nao autenticado em vez de
        // travar a tela em "Carregando..." para sempre.
        if (ativo) {
          router.replace("/admin/login");
        }
      });

    return () => {
      ativo = false;
    };
  }, [router]);

  return autorizado;
}

/** Trata 401 do servidor (sessao expirada/invalida): manda ao login. */
export function tratarNaoAutorizado(status: number, router: { replace: (url: string) => void }): boolean {
  if (status === 401) {
    router.replace("/admin/login");
    return true;
  }
  return false;
}
