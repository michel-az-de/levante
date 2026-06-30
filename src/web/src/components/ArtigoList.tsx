import { ArtigoCard } from "@/components/ArtigoCard";
import type { Artigo, Categoria } from "@/types/domain";

export function ArtigoList({
  artigos,
  categorias = [],
}: {
  artigos: Artigo[];
  categorias?: Categoria[];
}) {
  if (artigos.length === 0) {
    return <p className="text-neutral-500">Nenhum artigo publicado ainda.</p>;
  }

  const nomePorId = new Map(categorias.map((categoria) => [categoria.id, categoria.nome]));

  return (
    <div className="grid gap-4">
      {artigos.map((artigo) => (
        <ArtigoCard
          key={artigo.id}
          artigo={artigo}
          categoriaNome={artigo.categoriaId ? nomePorId.get(artigo.categoriaId) : undefined}
        />
      ))}
    </div>
  );
}
