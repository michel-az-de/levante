import type { Metadata } from "next";
import { ArtigoList } from "@/components/ArtigoList";
import { artigoApi } from "@/lib/api";

// ISR: HTML server-rendered, revalidado periodicamente (indexavel, sem exigir
// a API no build do CI).
export const revalidate = 300;

export const metadata: Metadata = {
  title: "Artigos",
  description: "Artigos tecnicos publicados no Levante.",
  alternates: { canonical: "/artigos" },
  openGraph: { title: "Artigos", url: "/artigos" },
};

export default async function ArtigosPage() {
  const { data, error } = await artigoApi.GET("/artigos");

  return (
    <main className="mx-auto flex min-h-screen max-w-3xl flex-col gap-6 px-6 py-16">
      <h1 className="text-3xl font-bold tracking-tight">Artigos</h1>
      {error ? (
        <p className="text-red-600">Nao foi possivel carregar os artigos.</p>
      ) : (
        <ArtigoList artigos={data ?? []} />
      )}
    </main>
  );
}
