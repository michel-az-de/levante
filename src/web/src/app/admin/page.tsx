"use client";

import Link from "next/link";
import { useAdminAuth, useSessaoAdmin } from "@/lib/admin-auth-context";
import { sairDoAdmin } from "@/lib/auth";

export default function AdminDashboardPage() {
  const autorizado = useAdminAuth();
  const sessao = useSessaoAdmin();

  async function sair() {
    await sairDoAdmin();
    // Navegacao completa (nao SPA): descarta a sessao em memoria do
    // AdminAuthProvider — voltar pelo historico nao reexibe o shell logado.
    window.location.replace("/admin/login");
  }

  if (!autorizado) {
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
        Logado como <span className="font-medium">{sessao?.email}</span>.
      </p>
      <nav className="flex flex-col gap-2">
        <Link
          href="/admin/artigos"
          className="rounded-md border border-neutral-300 px-4 py-3 transition hover:bg-neutral-100 dark:border-neutral-700 dark:hover:bg-neutral-900"
        >
          Gerenciar artigos
        </Link>
        <Link
          href="/admin/categorias"
          className="rounded-md border border-neutral-300 px-4 py-3 transition hover:bg-neutral-100 dark:border-neutral-700 dark:hover:bg-neutral-900"
        >
          Gerenciar categorias
        </Link>
        <Link
          href="/admin/comentarios"
          className="rounded-md border border-neutral-300 px-4 py-3 transition hover:bg-neutral-100 dark:border-neutral-700 dark:hover:bg-neutral-900"
        >
          Moderar comentarios
        </Link>
      </nav>
    </main>
  );
}
