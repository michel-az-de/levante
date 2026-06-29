import Link from "next/link";

export default function HomePage() {
  return (
    <main className="mx-auto flex min-h-screen max-w-3xl flex-col justify-center gap-6 px-6">
      <h1 className="text-4xl font-bold tracking-tight">Levante</h1>
      <p className="text-lg text-neutral-600 dark:text-neutral-400">
        Da pedra bruta a pedra polida. Walking skeleton (Fatia 0).
      </p>
      <Link
        href="/artigos"
        className="w-fit rounded-md bg-neutral-900 px-4 py-2 text-white transition hover:bg-neutral-700 dark:bg-white dark:text-neutral-900 dark:hover:bg-neutral-200"
      >
        Ver artigos
      </Link>
    </main>
  );
}
