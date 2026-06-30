"use client";

import Link from "next/link";
import { useRouter } from "next/navigation";
import { useEffect, useState } from "react";
import { apiAdmin, limparToken, obterToken } from "@/lib/auth";

export default function AdminDashboardPage() {
  const router = useRouter();
  const [email, setEmail] = useState<string | null>(null);
  const [carregando, setCarregando] = useState(true);

  useEffect(() => {
    if (!obterToken()) {
      router.replace("/admin/login");
      return;
    }

    let ativo = true;
    apiAdmin.GET("/auth/eu").then(({ data, error }) => {
      if (!ativo) {
        return;
      }
      if (error || !data) {
        limparToken();
        router.replace("/admin/login");
        return;
      }
      setEmail(data.email);
      setCarregando(false);
    });

    return () => {
      ativo = false;
    };
  }, [router]);

  function sair() {
    limparToken();
    router.replace("/admin/login");
  }

  if (carregando) {
    return (
      <main className="mx-auto flex min-h-screen max-w-3xl items-center justify-center px-6">
        <p className="text-neutral-500">Carregando...</p>
      </main>
    );
  }

  return (
    <main className="mx-auto flex min-h-screen max-w-3xl flex-col gap-6 px-6 py-16">
      <div className="flex items-center justify-between">
        <h1 className="text-3xl font-bold tracking-tight">Admin</h1>
        <button
          type="button"
          onClick={sair}
          className="rounded-md border border-neutral-300 px-3 py-1.5 text-sm transition hover:bg-neutral-100 dark:border-neutral-700 dark:hover:bg-neutral-900"
        >
          Sair
        </button>
      </div>
      <p className="text-neutral-600 dark:text-neutral-400">
        Logado como <span className="font-medium">{email}</span>.
      </p>
      <nav className="flex flex-col gap-2">
        <Link
          href="/admin/artigos"
          className="rounded-md border border-neutral-300 px-4 py-3 transition hover:bg-neutral-100 dark:border-neutral-700 dark:hover:bg-neutral-900"
        >
          Gerenciar artigos
        </Link>
      </nav>
    </main>
  );
}
