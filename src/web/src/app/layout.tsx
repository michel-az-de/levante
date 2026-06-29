import type { Metadata } from "next";
import "./globals.css";

export const metadata: Metadata = {
  title: "Levante",
  description: "Plataforma pessoal e portfolio tecnico de Felipe Michel de Azevedo.",
};

export default function RootLayout({
  children,
}: Readonly<{ children: React.ReactNode }>) {
  return (
    <html lang="pt-BR">
      <body>{children}</body>
    </html>
  );
}
