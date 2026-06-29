import { ArtigoList } from "@/components/ArtigoList";
import { artigoApi } from "@/lib/api";

// SSR a cada request na Fatia 0 (sem cache), provando o caminho front -> API.
export const dynamic = "force-dynamic";

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
