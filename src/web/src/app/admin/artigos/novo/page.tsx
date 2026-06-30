"use client";

import Link from "next/link";
import { useRouter } from "next/navigation";
import { ArtigoEditor, type ArtigoFormValores } from "@/components/ArtigoEditor";
import { useGuardaAdmin, tratarNaoAutorizado } from "@/lib/admin-guard";
import { apiAdmin } from "@/lib/auth";

export default function NovoArtigoPage() {
  const router = useRouter();
  const autorizado = useGuardaAdmin();

  async function criar(valores: ArtigoFormValores): Promise<string | null> {
    const { data, error, response } = await apiAdmin.POST("/artigos", {
      body: { ...valores, categoriaId: valores.categoriaId || undefined },
    });
    if (data && response.ok) {
      router.push("/admin/artigos");
      return null;
    }
    if (tratarNaoAutorizado(response.status, router)) {
      return null;
    }
    return error?.detail ?? "Falha ao criar o artigo.";
  }

  if (!autorizado) {
    return (
      <main className="mx-auto flex min-h-screen max-w-3xl items-center justify-center px-6">
        <p className="text-neutral-500">Carregando...</p>
      </main>
    );
  }

  return (
    <main className="mx-auto flex min-h-screen max-w-4xl flex-col gap-6 px-6 py-16">
      <div className="flex items-center justify-between">
        <h1 className="text-3xl font-bold tracking-tight">Novo artigo</h1>
        <Link href="/admin/artigos" className="text-sm text-neutral-500 hover:underline">
          Voltar
        </Link>
      </div>
      <ArtigoEditor
        inicial={{
          titulo: "",
          slug: "",
          resumo: "",
          conteudo: "",
          metaTitulo: "",
          metaDescricao: "",
          imagemOgUrl: "",
          categoriaId: "",
          tags: [],
        }}
        textoAcao="Criar"
        onSubmit={criar}
      />
    </main>
  );
}
