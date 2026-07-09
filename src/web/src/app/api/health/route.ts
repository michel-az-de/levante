import { NextResponse } from "next/server";

// Liveness do container web: alimenta o healthcheck do compose (stack conjunta na VM) e
// o smoke do deploy (GET /api/health). Afirma so que o processo Next esta servindo — nao
// toca a API .NET (a prontidao do Mongo/Hiram e responsabilidade de /health/ready da API).
// force-dynamic para nunca virar rota estatica/cacheada.
export const dynamic = "force-dynamic";

export function GET(): NextResponse {
  return NextResponse.json({ status: "ok" }, { status: 200 });
}
