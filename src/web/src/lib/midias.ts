import { apiAdmin } from "@/lib/auth";
import type { components } from "@/types/api";

export type MidiaEnviada = components["schemas"]["MidiaResponse"];

/**
 * Envia uma imagem para o admin (multipart) e retorna `{ id, url, ... }` —
 * `url` e relativa (`/midias/{id}`), pronta para uso no markdown do artigo.
 *
 * O schema do OpenAPI para este endpoint (IFormFile) nao modela multipart de
 * verdade — limitacao conhecida do openapi-typescript. O bodySerializer monta
 * o FormData de fato; o campo "arquivo" tem que casar com
 * MidiaAdminEndpoints.CampoArquivo no backend.
 */
export async function enviarMidia(arquivo: File): Promise<MidiaEnviada> {
  const { data, error, response } = await apiAdmin.POST("/admin/midias", {
    body: { arquivo } as never,
    bodySerializer(body) {
      const formulario = new FormData();
      for (const [nome, valor] of Object.entries(body as unknown as Record<string, Blob>)) {
        formulario.append(nome, valor);
      }
      return formulario;
    },
  });

  if (error || !data) {
    throw new Error(`Falha ao enviar midia (HTTP ${response.status}).`);
  }

  return data;
}
