"use client";

import Link from "next/link";
import { useRouter } from "next/navigation";
import { useCallback, useEffect, useState } from "react";
import { useGuardaAdmin, tratarNaoAutorizado } from "@/lib/admin-guard";
import { apiAdmin } from "@/lib/auth";
import type { Categoria } from "@/types/domain";

interface FormularioCategoria {
  id: string | null;
  nome: string;
  slug: string;
  descricao: string;
}

const FORM_VAZIO: FormularioCategoria = { id: null, nome: "", slug: "", descricao: "" };

export default function AdminCategoriasPage() {
  const router = useRouter();
  const autorizado = useGuardaAdmin();
  const [categorias, setCategorias] = useState<Categoria[] | null>(null);
  const [form, setForm] = useState<FormularioCategoria>(FORM_VAZIO);
  const [erro, setErro] = useState<string | null>(null);
  const [ocupado, setOcupado] = useState(false);

  const carregar = useCallback(() => {
    apiAdmin
      .GET("/categorias")
      .then(({ data, response }) => {
        if (tratarNaoAutorizado(response.status, router)) {
          return;
        }
        if (!response.ok) {
          setErro("Falha ao carregar as categorias.");
          setCategorias([]);
          return;
        }
        setCategorias(data ?? []);
      })
      .catch(() => {
        setErro("Falha de conexão ao carregar as categorias.");
        setCategorias([]);
      });
  }, [router]);

  useEffect(() => {
    if (autorizado) {
      carregar();
    }
  }, [autorizado, carregar]);

  async function salvar(evento: React.FormEvent) {
    evento.preventDefault();
    setErro(null);
    setOcupado(true);
    try {
      const resposta = form.id
        ? await apiAdmin.PUT("/categorias/{id}", {
            params: { path: { id: form.id } },
            body: { nome: form.nome, descricao: form.descricao || undefined },
          })
        : await apiAdmin.POST("/categorias", {
            body: { nome: form.nome, slug: form.slug, descricao: form.descricao || undefined },
          });

      if (tratarNaoAutorizado(resposta.response.status, router)) {
        return;
      }
      if (!resposta.data || !resposta.response.ok) {
        setErro(resposta.error?.detail ?? "Falha ao salvar a categoria.");
        return;
      }
      setForm(FORM_VAZIO);
      carregar();
    } catch {
      setErro("Falha de conexão. Tente novamente.");
    } finally {
      setOcupado(false);
    }
  }

  if (!autorizado || categorias === null) {
    return (
      <main className="mx-auto flex min-h-screen max-w-3xl items-center justify-center px-6">
        <p className="text-neutral-500">Carregando...</p>
      </main>
    );
  }

  return (
    <main className="mx-auto flex min-h-screen max-w-3xl flex-col gap-6 px-6 py-16">
      <div className="flex items-center justify-between">
        <h1 className="text-3xl font-bold tracking-tight">Categorias</h1>
        <Link href="/admin" className="text-sm text-neutral-500 hover:underline">
          Voltar
        </Link>
      </div>

      <form onSubmit={salvar} className="flex flex-col gap-3 rounded-md border border-neutral-200 p-4 dark:border-neutral-800">
        <h2 className="text-sm font-medium">{form.id ? "Editar categoria" : "Nova categoria"}</h2>
        <input
          type="text"
          required
          value={form.nome}
          onChange={(e) => setForm({ ...form, nome: e.target.value })}
          placeholder="Nome"
          className="rounded-md border border-neutral-300 px-3 py-2 dark:border-neutral-700 dark:bg-neutral-900"
        />
        <input
          type="text"
          required
          disabled={form.id !== null}
          value={form.slug}
          onChange={(e) => setForm({ ...form, slug: e.target.value })}
          placeholder="slug (kebab-case, imutável)"
          className="rounded-md border border-neutral-300 px-3 py-2 font-mono disabled:opacity-50 dark:border-neutral-700 dark:bg-neutral-900"
        />
        <textarea
          rows={2}
          value={form.descricao}
          onChange={(e) => setForm({ ...form, descricao: e.target.value })}
          placeholder="Descrição (opcional)"
          className="rounded-md border border-neutral-300 px-3 py-2 dark:border-neutral-700 dark:bg-neutral-900"
        />
        {erro ? <p className="text-sm text-red-600">{erro}</p> : null}
        <div className="flex gap-2">
          <button
            type="submit"
            disabled={ocupado}
            className="rounded-md bg-neutral-900 px-4 py-2 text-sm text-white transition hover:bg-neutral-700 disabled:opacity-50 dark:bg-white dark:text-neutral-900 dark:hover:bg-neutral-200"
          >
            {form.id ? "Salvar" : "Criar"}
          </button>
          {form.id ? (
            <button
              type="button"
              onClick={() => setForm(FORM_VAZIO)}
              className="rounded-md border border-neutral-300 px-4 py-2 text-sm transition hover:bg-neutral-100 dark:border-neutral-700 dark:hover:bg-neutral-900"
            >
              Cancelar
            </button>
          ) : null}
        </div>
      </form>

      {categorias.length === 0 ? (
        <p className="text-neutral-500">Nenhuma categoria ainda.</p>
      ) : (
        <ul className="flex flex-col divide-y divide-neutral-200 dark:divide-neutral-800">
          {categorias.map((categoria) => (
            <li key={categoria.id} className="flex items-center justify-between gap-4 py-3">
              <div className="flex flex-col gap-1">
                <span className="font-medium">{categoria.nome}</span>
                <span className="font-mono text-xs text-neutral-500">{categoria.slug}</span>
              </div>
              <button
                type="button"
                onClick={() =>
                  setForm({
                    id: categoria.id,
                    nome: categoria.nome,
                    slug: categoria.slug,
                    descricao: categoria.descricao ?? "",
                  })
                }
                className="rounded border border-neutral-300 px-2 py-1 text-xs transition hover:bg-neutral-100 dark:border-neutral-700 dark:hover:bg-neutral-800"
              >
                Editar
              </button>
            </li>
          ))}
        </ul>
      )}
    </main>
  );
}
