import type { Metadata } from "next";
import { FormInscricaoNewsletter } from "@/components/FormInscricaoNewsletter";
import { newsletterHabilitada } from "@/lib/flags";

export const metadata: Metadata = {
  title: "Newsletter",
  description: "Receba os novos artigos do Levante por e-mail.",
  alternates: { canonical: "/newsletter" },
};

// force-dynamic: le NEWSLETTER_ENABLED em runtime (ativado no cutover, sem rebuild).
export const dynamic = "force-dynamic";

export default function NewsletterPage() {
  const habilitada = newsletterHabilitada();

  return (
    <main className="mx-auto flex min-h-screen max-w-2xl flex-col justify-center gap-6 px-6 py-16">
      <header className="flex flex-col gap-2">
        <h1 className="text-3xl font-bold tracking-tight">Newsletter</h1>
        <p className="text-neutral-600 dark:text-neutral-400">
          Receba os novos artigos por e-mail. Sem spam; cancele quando quiser.
        </p>
      </header>

      {habilitada ? (
        <>
          <FormInscricaoNewsletter />
          <p className="text-xs text-neutral-500">
            Ao se inscrever, voce consente em receber e-mails do Levante. Confirmamos seu e-mail
            antes de enviar qualquer coisa (double opt-in) e cada mensagem traz um link de
            descadastro.
          </p>
        </>
      ) : (
        <p className="text-neutral-600 dark:text-neutral-400">
          A newsletter estara disponivel em breve. Volte logo.
        </p>
      )}
    </main>
  );
}
