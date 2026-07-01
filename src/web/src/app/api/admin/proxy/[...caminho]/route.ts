import { cookies } from "next/headers";
import { COOKIE_SESSAO_ADMIN, apiBaseUrl } from "@/lib/sessao-admin";

/**
 * BFF do admin: repassa a chamada para a API anexando o JWT do cookie httpOnly
 * como Bearer. Sem cookie -> 401 (o guard do front redireciona ao login).
 * Mutacoes de origem cruzada -> 403 (reforco do SameSite=strict).
 */

async function repassar(request: Request, caminho: string[]): Promise<Response> {
  const mutacao = request.method !== "GET" && request.method !== "HEAD";
  if (mutacao && origemCruzada(request)) {
    return new Response(null, { status: 403 });
  }

  const jar = await cookies();
  const token = jar.get(COOKIE_SESSAO_ADMIN)?.value;
  if (!token) {
    return new Response(null, { status: 401 });
  }

  const url = new URL(request.url);
  const destino = `${apiBaseUrl()}/${caminho.map(encodeURIComponent).join("/")}${url.search}`;

  const cabecalhos = new Headers({ Authorization: `Bearer ${token}` });
  const contentType = request.headers.get("content-type");
  if (contentType) {
    cabecalhos.set("Content-Type", contentType);
  }

  const resposta = await fetch(destino, {
    method: request.method,
    headers: cabecalhos,
    body: mutacao ? await request.arrayBuffer() : undefined,
    cache: "no-store",
  });

  const saida = new Headers();
  const tipoResposta = resposta.headers.get("content-type");
  if (tipoResposta) {
    saida.set("Content-Type", tipoResposta);
  }
  return new Response(resposta.body, { status: resposta.status, headers: saida });
}

function origemCruzada(request: Request): boolean {
  const origem = request.headers.get("origin");
  if (!origem) {
    return false;
  }
  try {
    return new URL(origem).host !== new URL(request.url).host;
  } catch {
    return true;
  }
}

type Contexto = { params: Promise<{ caminho: string[] }> };

export async function GET(request: Request, contexto: Contexto): Promise<Response> {
  return repassar(request, (await contexto.params).caminho);
}

export async function POST(request: Request, contexto: Contexto): Promise<Response> {
  return repassar(request, (await contexto.params).caminho);
}

export async function PUT(request: Request, contexto: Contexto): Promise<Response> {
  return repassar(request, (await contexto.params).caminho);
}

export async function PATCH(request: Request, contexto: Contexto): Promise<Response> {
  return repassar(request, (await contexto.params).caminho);
}

export async function DELETE(request: Request, contexto: Contexto): Promise<Response> {
  return repassar(request, (await contexto.params).caminho);
}
