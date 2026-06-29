import { ArtigoCard } from "@/components/ArtigoCard";
import type { Artigo } from "@/types/domain";

export function ArtigoList({ artigos }: { artigos: Artigo[] }) {
  if (artigos.length === 0) {
    return (
      <p className="text-neutral-500">Nenhum artigo publicado ainda.</p>
    );
  }

  return (
    <div className="grid gap-4">
      {artigos.map((artigo) => (
        <ArtigoCard key={artigo.id} artigo={artigo} />
      ))}
    </div>
  );
}
