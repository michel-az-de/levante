"use client";

import Link from "next/link";
import { useParams, useRouter } from "next/navigation";
import { useEffect, useState } from "react";
import { ArtigoEditor, type ArtigoFormValores } from "@/components/ArtigoEditor";
import { useGuardaAdmin, tratarNaoAutorizado } from "@/lib/admin-guard";
import { apiAdmin } from "@/lib/auth";

export default function EditarArtigoPage() {
  const router = useRouter();
  const params = useParams();
  const id = String(params.id);
  const autorizado = useGuardaAdmin();
  const [inicial, setInicial] = useState<ArtigoFormValores | null>(null);
  const [naoEncontrado, setNaoEncontrado] = useState(false);
  const [erroCarregar, setErroCarregar] = useState(false);

  useEffect(() => {
    if (!autorizado) {
      return;
    }
    let ativo = true;
    // Sem GET admin por id (fora de escopo da 2b): busca na lista e filtra.
    apiAdmin
      .GET("/admin/artigos")
      .then(({ data, response }) => {
        if (!ativo) {
          return;
        }
        if (tratarNaoAutorizado(response.status, router)) {
          return;
        }
        // Erro do servidor != artigo inexistente: nao mascarar um 500 como 404.
        if (!response.ok) {
          setErroCarregar(true);
          return;
        }
        const artigo = data?.find((a) => a.id === id);
        if (!artigo) {
          setNaoEncontrado(true);
          return;
        }
        setInicial({
          titulo: artigo.titulo,
          slug: artigo.slug,
          resumo: artigo.resumo,
          conteudo: artigo.conteudo,
          metaTitulo: artigo.metaTitulo ?? "",
          metaDescricao: artigo.metaDescricao ?? "",
          imagemOgUrl: artigo.imagemOgUrl ?? "",
        });
      })
      .catch(() => {
        if (ativo) {
          setErroCarregar(true);
        }
      });
    return () => {
      ativo = false;
    };
  }, [autorizado, id, router]);

  async function salvar(valores: ArtigoFormValores): Promise<string | null> {
    const { data, error, response } = await apiAdmin.PUT("/artigos/{id}", {
      params: { path: { id } },
      body: valores,
    });
    if (data && response.ok) {
      router.push("/admin/artigos");
      return null;
    }
    if (tratarNaoAutorizado(response.status, router)) {
      return null;
    }
    return error?.detail ?? "Falha ao salvar o artigo.";
  }

  if (!autorizado || (!inicial && !naoEncontrado && !erroCarregar)) {
    return (
      <main className="mx-auto flex min-h-screen max-w-3xl items-center justify-center px-6">
        <p className="text-neutral-500">Carregando...</p>
      </main>
    );
  }

  return (
    <main className="mx-auto flex min-h-screen max-w-4xl flex-col gap-6 px-6 py-16">
      <div className="flex items-center justify-between">
        <h1 className="text-3xl font-bold tracking-tight">Editar artigo</h1>
        <Link href="/admin/artigos" className="text-sm text-neutral-500 hover:underline">
          Voltar
        </Link>
      </div>
      {erroCarregar ? (
        <p className="text-sm text-red-600">Falha ao carregar o artigo. Tente novamente.</p>
      ) : naoEncontrado || !inicial ? (
        <p className="text-neutral-500">Artigo nao encontrado.</p>
      ) : (
        <ArtigoEditor inicial={inicial} textoAcao="Salvar" onSubmit={salvar} />
      )}
    </main>
  );
}
