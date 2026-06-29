import Link from "next/link";

export default function NotFound() {
  return (
    <main className="mx-auto flex min-h-screen max-w-3xl flex-col justify-center gap-4 px-6">
      <h1 className="text-4xl font-bold tracking-tight">404</h1>
      <p className="text-lg text-neutral-600 dark:text-neutral-400">
        Pagina nao encontrada.
      </p>
      <Link href="/" className="w-fit text-neutral-900 underline dark:text-neutral-100">
        Voltar ao inicio
      </Link>
    </main>
  );
}
