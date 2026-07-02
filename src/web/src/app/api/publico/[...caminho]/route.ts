import { cookies } from "next/headers";
import { NextResponse } from "next/server";
import { apiBaseUrl } from "@/lib/sessao-admin";
import {
  COOKIE_VISITANTE,
  HEADER_VISITANTE,
  VISITANTE_MAX_AGE_SEGUNDOS,
} from "@/lib/visitante";

/**
 * BFF publico das escritas anonimas (reacoes/comentarios). Mantem a invariante
 * da Fatia A5 (o browser nunca fala com a API .NET direto): repassa a chamada
 * anexando o id de visitante (cookie httpOnly first-party) e o IP do cliente.
 * Nao exige auth — os endpoints da API sao publicos e rate-limited.
 */

async function repassar(request: Request, caminho: string[]): Promise<NextResponse> {
  const cookieStore = await cookies();

  let visitante = cookieStore.get(COOKIE_VISITANTE)?.value;
  if (!visitante) {
    visitante = crypto.randomUUID();
  }

  const url = new URL(request.url);
  const destino = `${apiBaseUrl()}/${caminho.map(encodeURIComponent).join("/")}${url.search}`;

  const cabecalhos = new Headers({ [HEADER_VISITANTE]: visitante });
  const contentType = request.headers.get("content-type");
  if (contentType) {
    cabecalhos.set("Content-Type", contentType);
  }
  // IP real do cliente (posto pelo proxy/ingress em producao). A API o usa so
  // como sinal secundario (hash), nunca guarda cru.
  const encaminhado = request.headers.get("x-forwarded-for");
  if (encaminhado) {
    cabecalhos.set("X-Forwarded-For", encaminhado);
  }

  const mutacao = request.method !== "GET" && request.method !== "HEAD";
  const resposta = await fetch(destino, {
    method: request.method,
    headers: cabecalhos,
    body: mutacao ? await request.arrayBuffer() : undefined,
    cache: "no-store",
  });

  const saida = new NextResponse(resposta.body, { status: resposta.status });
  const tipoResposta = resposta.headers.get("content-type");
  if (tipoResposta) {
    saida.headers.set("Content-Type", tipoResposta);
  }

  // Emite/renova o cookie de visitante (httpOnly, first-party).
  saida.cookies.set({
    name: COOKIE_VISITANTE,
    value: visitante,
    httpOnly: true,
    secure: process.env.NODE_ENV === "production",
    sameSite: "lax",
    path: "/",
    maxAge: VISITANTE_MAX_AGE_SEGUNDOS,
  });

  return saida;
}

type Contexto = { params: Promise<{ caminho: string[] }> };

export async function GET(request: Request, contexto: Contexto): Promise<NextResponse> {
  return repassar(request, (await contexto.params).caminho);
}

export async function POST(request: Request, contexto: Contexto): Promise<NextResponse> {
  return repassar(request, (await contexto.params).caminho);
}

export async function DELETE(request: Request, contexto: Contexto): Promise<NextResponse> {
  return repassar(request, (await contexto.params).caminho);
}
