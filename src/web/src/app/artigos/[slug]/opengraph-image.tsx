import { ImageResponse } from "next/og";
import { artigoApi } from "@/lib/api";
import { site } from "@/lib/site";

export const size = { width: 1200, height: 630 };
export const contentType = "image/png";
export const alt = "Artigo do Levante";

export default async function OpenGraphImage({
  params,
}: {
  params: Promise<{ slug: string }>;
}) {
  const { slug } = await params;

  let titulo: string = site.nome;
  try {
    const { data } = await artigoApi.GET("/artigos/{slug}", {
      params: { path: { slug } },
    });
    if (data) {
      titulo = data.titulo;
    }
  } catch {
    // OG cai para o nome do site se a API estiver indisponivel.
  }

  return new ImageResponse(
    (
      <div
        style={{
          width: "100%",
          height: "100%",
          display: "flex",
          flexDirection: "column",
          justifyContent: "space-between",
          padding: 96,
          background: "#0a0a0a",
          color: "#fafafa",
        }}
      >
        <div style={{ fontSize: 28, color: "#a3a3a3" }}>{site.nome}</div>
        <div style={{ fontSize: 64, fontWeight: 700, lineHeight: 1.1 }}>{titulo}</div>
      </div>
    ),
    size,
  );
}
