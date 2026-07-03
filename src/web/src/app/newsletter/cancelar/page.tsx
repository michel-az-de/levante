import type { Metadata } from "next";
import { AcaoNewsletter } from "@/components/AcaoNewsletter";

export const metadata: Metadata = {
  title: "Cancelar inscricao",
  robots: { index: false },
};

export default async function CancelarNewsletterPage({
  searchParams,
}: {
  searchParams: Promise<{ token?: string }>;
}) {
  const { token } = await searchParams;

  return (
    <main className="mx-auto flex min-h-screen max-w-xl flex-col justify-center gap-4 px-6 py-16">
      <h1 className="text-2xl font-bold tracking-tight">Cancelamento da newsletter</h1>
      <AcaoNewsletter token={token ?? ""} acao="cancelar" />
    </main>
  );
}
