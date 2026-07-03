import type { Metadata } from "next";
import { FormInscricaoNewsletter } from "@/components/FormInscricaoNewsletter";

export const metadata: Metadata = {
  title: "Newsletter",
  description: "Receba os novos artigos do Levante por e-mail.",
  alternates: { canonical: "/newsletter" },
};

export default function NewsletterPage() {
  return (
    <main className="mx-auto flex min-h-screen max-w-2xl flex-col justify-center gap-6 px-6 py-16">
      <header className="flex flex-col gap-2">
        <h1 className="text-3xl font-bold tracking-tight">Newsletter</h1>
        <p className="text-neutral-600 dark:text-neutral-400">
          Receba os novos artigos por e-mail. Sem spam; cancele quando quiser.
        </p>
      </header>

      <FormInscricaoNewsletter />

      <p className="text-xs text-neutral-500">
        Ao se inscrever, voce consente em receber e-mails do Levante. Confirmamos seu e-mail antes de
        enviar qualquer coisa (double opt-in) e cada mensagem traz um link de descadastro.
      </p>
    </main>
  );
}
