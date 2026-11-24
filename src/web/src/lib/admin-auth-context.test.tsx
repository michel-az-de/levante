import { cleanup, render, screen, waitFor } from "@testing-library/react";
import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import {
  AdminAuthProvider,
  tratarNaoAutorizado,
  useAdminAuth,
  useSessaoAdmin,
} from "@/lib/admin-auth-context";

const { replace, obterSessao, rota } = vi.hoisted(() => ({
  replace: vi.fn(),
  obterSessao: vi.fn(),
  rota: { pathname: "/admin" },
}));

vi.mock("next/navigation", () => ({
  useRouter: () => ({ replace }),
  usePathname: () => rota.pathname,
}));

vi.mock("@/lib/auth", () => ({
  apiAdmin: { GET: obterSessao },
}));

/** Consumidor minimo dos hooks, para observar o estado do contexto. */
function Sonda() {
  const autorizado = useAdminAuth();
  const sessao = useSessaoAdmin();
  return <p>{autorizado ? `autorizado:${sessao?.email}` : "bloqueado"}</p>;
}

describe("AdminAuthProvider", () => {
  beforeEach(() => {
    replace.mockReset();
    obterSessao.mockReset();
    rota.pathname = "/admin";
  });

  afterEach(() => {
    cleanup();
  });

  it("valida a sessao e compartilha o e-mail com os filhos", async () => {
    obterSessao.mockResolvedValue({ data: { email: "admin@levante.dev" } });

    render(
      <AdminAuthProvider>
        <Sonda />
      </AdminAuthProvider>,
    );

    await waitFor(() => expect(screen.getByText("autorizado:admin@levante.dev")).toBeTruthy());
    expect(obterSessao).toHaveBeenCalledWith("/auth/eu");
    expect(replace).not.toHaveBeenCalled();
  });

  it("redireciona ao login quando a sessao e invalida", async () => {
    obterSessao.mockResolvedValue({ data: undefined, error: { status: 401 } });

    render(
      <AdminAuthProvider>
        <Sonda />
      </AdminAuthProvider>,
    );

    await waitFor(() => expect(replace).toHaveBeenCalledWith("/admin/login"));
    expect(screen.getByText("bloqueado")).toBeTruthy();
  });

  it("na tela de login nao valida sessao nem redireciona", async () => {
    rota.pathname = "/admin/login";

    render(
      <AdminAuthProvider>
        <Sonda />
      </AdminAuthProvider>,
    );

    await waitFor(() => expect(screen.getByText("bloqueado")).toBeTruthy());
    expect(obterSessao).not.toHaveBeenCalled();
    expect(replace).not.toHaveBeenCalled();
  });

  it("nao repete /auth/eu ao navegar entre telas ja autorizado", async () => {
    obterSessao.mockResolvedValue({ data: { email: "admin@levante.dev" } });

    const { rerender } = render(
      <AdminAuthProvider>
        <Sonda />
      </AdminAuthProvider>,
    );
    await waitFor(() => expect(screen.getByText(/autorizado/)).toBeTruthy());

    rota.pathname = "/admin/artigos";
    rerender(
      <AdminAuthProvider>
        <Sonda />
      </AdminAuthProvider>,
    );

    await waitFor(() => expect(screen.getByText(/autorizado/)).toBeTruthy());
    expect(obterSessao).toHaveBeenCalledTimes(1);
  });
});

describe("tratarNaoAutorizado (401 do servidor)", () => {
  it("em 401 redireciona para o login", () => {
    const replaceLocal = vi.fn();

    const tratado = tratarNaoAutorizado(401, { replace: replaceLocal });

    expect(tratado).toBe(true);
    expect(replaceLocal).toHaveBeenCalledWith("/admin/login");
  });

  it("em outros status nao redireciona", () => {
    const replaceLocal = vi.fn();

    const tratado = tratarNaoAutorizado(500, { replace: replaceLocal });

    expect(tratado).toBe(false);
    expect(replaceLocal).not.toHaveBeenCalled();
  });
});
