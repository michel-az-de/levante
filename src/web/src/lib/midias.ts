import { apiAdmin } from "@/lib/auth";
import type { components } from "@/types/api";

export type MidiaEnviada = components["schemas"]["MidiaResponse"];

/**
 * Erro de upload/remocao de midia que carrega o status HTTP, para a UI distinguir
 * casos (ex.: 413 = arquivo maior que o limite) sem depender do texto da mensagem.
 */
export class ErroDeMidia extends Error {
  constructor(
    mensagem: string,
    readonly status: number,
  ) {
    super(mensagem);
    this.name = "ErroDeMidia";
  }
}

/**
 * Envia uma imagem para o admin (multipart) e retorna `{ id, url, ... }` —
 * `url` e relativa (`/midias/{id}`), pronta para uso no markdown do artigo.
 *
 * O schema do OpenAPI para este endpoint (IFormFile) nao modela multipart de
 * verdade — o openapi-typescript tipa o corpo como string, dai o `as never`.
 * O serializer fecha sobre o File original em vez de recuperá-lo do corpo
 * tipado errado, entao nao precisa de cast nenhum. O campo "arquivo" tem que
 * casar com MidiaAdminEndpoints.CampoArquivo no backend.
 */
export async function enviarMidia(arquivo: File): Promise<MidiaEnviada> {
  const { data, error, response } = await apiAdmin.POST("/admin/midias", {
    body: { arquivo } as never,
    bodySerializer() {
      const formulario = new FormData();
      formulario.append("arquivo", arquivo);
      return formulario;
    },
  });

  if (error || !data) {
    throw new ErroDeMidia(`Falha ao enviar midia (HTTP ${response.status}).`, response.status);
  }

  return data;
}

/** Remove uma midia enviada. Copias ja cacheadas pelo browser nao sao recolhidas. */
export async function removerMidia(id: string): Promise<void> {
  const { error, response } = await apiAdmin.DELETE("/admin/midias/{id}", {
    params: { path: { id } },
  });

  if (error) {
    throw new ErroDeMidia(`Falha ao remover midia (HTTP ${response.status}).`, response.status);
  }
}
