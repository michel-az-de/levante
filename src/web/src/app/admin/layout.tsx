"use client";

import { AdminAuthProvider } from "@/lib/admin-auth-context";

/**
 * Layout admin: compartilha a autenticacao entre todas as telas. O login tem
 * rota propria e nao e afetado pelo guard.
 */
export default function AdminLayout({ children }: { children: React.ReactNode }) {
  return <AdminAuthProvider>{children}</AdminAuthProvider>;
}
