import { apiBaseUrl } from "@/lib/sessao-admin";

/**
 * BFF publico de midia: repassa o GET para a API .NET, propagando cache
 * imutavel/ETag e suportando 304 (If-None-Match). Mantem a invariante do site
 * (o browser nunca fala com a API direto) sem reimplementar streaming: o
 * corpo passa cru (Response.body).
 */
const CABECALHOS_REPASSADOS = ["content-type", "cache-control", "etag", "content-length"];

type Contexto = { params: Promise<{ id: string }> };

export async function GET(request: Request, contexto: Contexto): Promise<Response> {
  const { id } = await contexto.params;
  const destino = `${apiBaseUrl()}/midias/${encodeURIComponent(id)}`;

  const cabecalhos = new Headers();
  const seNaoCoincide = request.headers.get("if-none-match");
  if (seNaoCoincide) {
    cabecalhos.set("If-None-Match", seNaoCoincide);
  }

  const resposta = await fetch(destino, { headers: cabecalhos, cache: "no-store" });

  const saida = new Headers();
  for (const nome of CABECALHOS_REPASSADOS) {
    const valor = resposta.headers.get(nome);
    if (valor) {
      saida.set(nome, valor);
    }
  }

  return new Response(resposta.body, { status: resposta.status, headers: saida });
}
