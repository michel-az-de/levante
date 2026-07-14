import { NextResponse } from "next/server";
import { criarLimitador } from "@/lib/rate-limit-memoria";

// Recebe as violacoes da CSP (Content-Security-Policy-Report-Only) e loga o JSON no
// stdout do container -> coletado pelo Loki (D1). Endurecido (endpoint publico, nao
// autenticado): rate limit por IP, so tipos de report plausiveis, cap de tamanho e log
// truncado, para nao virar vetor de log flooding / custo de ingestao no Loki.
export const runtime = "nodejs";
export const dynamic = "force-dynamic";

const MAX_BYTES = 16 * 1024;
// Content-types que um navegador de fato usa para CSP report (report-uri e Reporting API).
const TIPOS_REPORT = ["application/csp-report", "application/reports+json"];
const permitir = criarLimitador(30, 60_000); // 30 reports/min por IP

function ipDoCliente(request: Request): string {
  const xff = request.headers.get("x-forwarded-for");
  return xff?.split(",")[0]?.trim() || "desconhecido";
}

export async function POST(request: Request): Promise<NextResponse> {
  if (!permitir(ipDoCliente(request), Date.now())) {
    return new NextResponse(null, { status: 429 });
  }

  const tipo = request.headers.get("content-type")?.split(";")[0]?.trim().toLowerCase() ?? "";
  if (!TIPOS_REPORT.includes(tipo)) {
    return new NextResponse(null, { status: 204 });
  }

  const declarado = Number(request.headers.get("content-length") ?? "0");
  if (Number.isFinite(declarado) && declarado > MAX_BYTES) {
    return new NextResponse(null, { status: 413 });
  }

  try {
    const corpo = await request.text();
    if (corpo) {
      // trunca o que vai pro log mesmo se o content-length mentir sobre o tamanho
      console.warn("[csp-report]", corpo.slice(0, MAX_BYTES));
    }
  } catch {
    // corpo malformado: ignora (o navegador nao repete o report)
  }
  return new NextResponse(null, { status: 204 });
}
