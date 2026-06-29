import createClient from "openapi-fetch";
import type { paths } from "@/types/api";

/**
 * Cliente tipado da API (tipos gerados do OpenAPI). Consumido no server
 * (SSR/SSG) pelos componentes de dominio.
 */
const baseUrl = process.env.API_BASE_URL ?? "http://localhost:5080";

export const artigoApi = createClient<paths>({ baseUrl });
