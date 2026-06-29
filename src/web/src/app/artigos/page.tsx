import type { Metadata } from "next";
import { ArtigoList } from "@/components/ArtigoList";
import { artigoApi } from "@/lib/api";
import type { Artigo } from "@/types/domain";

// SSR a cada request (HTML server-rendered, indexavel). Dinamico para a lista
// estar sempre fresca e para nao exigir a API no `next build` do CI.
export const dynamic = "force-dynamic";

export const metadata: Metadata = {
  title: "Artigos",
  description: "Artigos tecnicos publicados no Levante.",
  alternates: { canonical: "/artigos" },
  openGraph: { title: "Artigos", url: "/artigos" },
};

export default async function ArtigosPage() {
  let artigos: Artigo[] = [];
  let erro = false;
  try {
    const { data, error } = await artigoApi.GET("/artigos");
    if (error) {
      erro = true;
    } else {
      artigos = data ?? [];
    }
  } catch {
    erro = true;
  }

  return (
    <main className="mx-auto flex min-h-screen max-w-3xl flex-col gap-6 px-6 py-16">
      <h1 className="text-3xl font-bold tracking-tight">Artigos</h1>
      {erro ? (
        <p className="text-red-600">Nao foi possivel carregar os artigos.</p>
      ) : (
        <ArtigoList artigos={artigos} />
      )}
    </main>
  );
}
