/**
 * Skeleton generico do admin. Exibido enquanto a sessao e validada e os dados
 * sao carregados. Mantido neutro (sem site-* ou produto-*) para alinhar com o
 * tema neutro das telas de admin.
 */
export default function AdminLoading() {
  return (
    <main className="mx-auto flex min-h-screen max-w-3xl items-center justify-center px-6">
      <p className="text-neutral-500">Carregando...</p>
    </main>
  );
}
