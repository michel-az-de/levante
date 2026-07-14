import { NextResponse } from "next/server";

// Recebe as violacoes da CSP (Content-Security-Policy-Report-Only) e loga o JSON no
// stdout do container -> coletado pelo Loki (D1). Sem destino duravel a violacao
// morre no console do visitante e o endurecimento nunca tem dados.
export const runtime = "nodejs";
export const dynamic = "force-dynamic";

export async function POST(request: Request): Promise<NextResponse> {
  try {
    const corpo = await request.text();
    if (corpo) {
      console.warn("[csp-report]", corpo);
    }
  } catch {
    // corpo malformado: ignora (o navegador nao repete o report)
  }
  return new NextResponse(null, { status: 204 });
}
