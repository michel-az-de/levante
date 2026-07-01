import { NextResponse } from "next/server";
import {
  COOKIE_SESSAO_ADMIN,
  SESSAO_MAX_AGE_PADRAO_SEGUNDOS,
  apiBaseUrl,
} from "@/lib/sessao-admin";

/**
 * BFF da sessao do admin. POST troca credenciais por cookie httpOnly com o JWT
 * (o browser nunca ve o token); DELETE expira o cookie. SameSite=strict + check
 * de Origin cobrem CSRF; secure em producao.
 */

function atributosDoCookie(maxAge: number) {
  return {
    name: COOKIE_SESSAO_ADMIN,
    httpOnly: true,
    secure: process.env.NODE_ENV === "production",
    sameSite: "strict",
    path: "/",
    maxAge,
  } as const;
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

export async function POST(request: Request): Promise<NextResponse> {
  if (origemCruzada(request)) {
    return new NextResponse(null, { status: 403 });
  }

  const credenciais = await request.text();
  const resposta = await fetch(`${apiBaseUrl()}/auth/login`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: credenciais,
    cache: "no-store",
  });

  if (!resposta.ok) {
    // 401 para qualquer falha, igual a API (sem enumeracao).
    return new NextResponse(null, { status: 401 });
  }

  const token = (await resposta.json()) as { accessToken: string; expiraEmSegundos?: number };
  const saida = new NextResponse(null, { status: 204 });
  saida.cookies.set({
    ...atributosDoCookie(token.expiraEmSegundos ?? SESSAO_MAX_AGE_PADRAO_SEGUNDOS),
    value: token.accessToken,
  });
  return saida;
}

export function DELETE(request: Request): NextResponse {
  if (origemCruzada(request)) {
    return new NextResponse(null, { status: 403 });
  }

  const saida = new NextResponse(null, { status: 204 });
  saida.cookies.set({ ...atributosDoCookie(0), value: "" });
  return saida;
}
