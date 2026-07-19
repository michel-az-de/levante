import { apiBaseUrl } from "@/lib/sessao-admin";

/**
 * BFF publico de midia: repassa o GET para a API .NET, propagando cache
 * imutavel/ETag e suportando 304 (If-None-Match). Mantem a invariante do site
 * (o browser nunca fala com a API direto) sem reimplementar streaming: o
 * corpo passa cru (Response.body).
 */

// nosniff entra aqui porque a resposta e conteudo enviado por usuario: sem ele,
// um arquivo polyglot (GIF valido que tambem e HTML valido) poderia ser
// interpretado como documento pelo browser. A API ja emite o header; se ele nao
// estivesse nesta lista, morreria no repasse.
const CABECALHOS_REPASSADOS = [
  "content-type",
  "cache-control",
  "etag",
  "content-length",
  "x-content-type-options",
];

/** A API e vizinha na mesma VM; se nao responder rapido, e porque nao vai responder. */
const TIMEOUT_MS = 10_000;

type Contexto = { params: Promise<{ id: string }> };

export async function GET(request: Request, contexto: Contexto): Promise<Response> {
  const { id } = await contexto.params;
  const destino = `${apiBaseUrl()}/midias/${encodeURIComponent(id)}`;

  const cabecalhos = new Headers();
  const seNaoCoincide = request.headers.get("if-none-match");
  if (seNaoCoincide) {
    cabecalhos.set("If-None-Match", seNaoCoincide);
  }
  // Sem isto o rate limit global da API (particionado por RemoteIpAddress) ve o IP
  // do container do Next em TODA requisicao de imagem: um artigo com 10 imagens
  // esgota o balde de 100 req/min compartilhado por todos os visitantes. Mesmo
  // repasse que o BFF publico ja faz (ver api/publico/[...caminho]/route.ts).
  const encaminhado = request.headers.get("x-forwarded-for");
  if (encaminhado) {
    cabecalhos.set("X-Forwarded-For", encaminhado);
  }

  let resposta: Response;
  try {
    resposta = await fetch(destino, {
      headers: cabecalhos,
      cache: "no-store",
      signal: AbortSignal.timeout(TIMEOUT_MS),
    });
  } catch {
    // API fora do ar ou lenta demais. Sem este catch o route handler estoura e o
    // Next devolve a pagina de erro HTML dentro de uma tag <img> — 500 com corpo
    // enganoso. 502 vazio deixa o browser mostrar imagem quebrada, que e honesto.
    return new Response(null, { status: 502 });
  }

  const saida = new Headers();
  for (const nome of CABECALHOS_REPASSADOS) {
    const valor = resposta.headers.get(nome);
    if (valor) {
      saida.set(nome, valor);
    }
  }

  return new Response(resposta.body, { status: resposta.status, headers: saida });
}
