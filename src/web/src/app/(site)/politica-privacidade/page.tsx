import type { Metadata } from "next";
import Link from "next/link";
import { Idioma } from "@/components/Idioma";
import { site } from "@/lib/site";

const EMAIL = "felipe.azevedoit@gmail.com";

export const metadata: Metadata = {
  title: "Política de Privacidade",
  description:
    "Como o site trata seus dados: newsletter com consentimento, comentários moderados e logs de servidor. Sem rastreamento de terceiros.",
  alternates: { canonical: "/politica-privacidade" },
  openGraph: { title: "Política de Privacidade", url: `${site.url}/politica-privacidade` },
};

function Secao({ titulo, children }: { titulo: string; children: React.ReactNode }) {
  return (
    <section className="mt-10">
      <h2 className="mb-3 text-[clamp(20px,3vw,26px)] font-bold tracking-tight text-site-fg">
        {titulo}
      </h2>
      <div className="flex flex-col gap-3 text-[16.5px] leading-[1.7] text-site-fg2">{children}</div>
    </section>
  );
}

export default function PoliticaPrivacidadePage() {
  return (
    <div className="mx-auto max-w-[760px] px-[clamp(18px,4vw,40px)] py-[clamp(48px,8vw,92px)]">
      <Link
        href="/"
        className="inline-flex items-center gap-2 font-site-mono text-[12.5px] text-site-fg2 transition-colors hover:text-site-acc"
      >
        ← <Idioma pt="Voltar ao início" en="Back home" />
      </Link>

      <h1 className="mt-6 mb-2 text-[clamp(32px,5vw,52px)] leading-[1.05] font-bold tracking-[-0.03em] text-site-fg">
        Política de Privacidade
      </h1>
      <p className="font-site-mono text-[12.5px] text-site-faint">
        Vigência: julho de 2026 · conforme a LGPD (Lei 13.709/2018)
      </p>

      <Secao titulo="Quem trata seus dados">
        <p>
          Este site é a plataforma pessoal de <strong className="text-site-fg">{site.autor}</strong>.
          O tratamento de dados descrito abaixo é feito por ele, na qualidade de controlador, e você
          pode falar sobre qualquer ponto desta política pelo e-mail{" "}
          <a href={`mailto:${EMAIL}`} className="text-site-acc underline-offset-2 hover:underline">
            {EMAIL}
          </a>
          .
        </p>
      </Secao>

      <Secao titulo="Que dados são coletados, e por quê">
        <p>
          <strong className="text-site-fg">Newsletter.</strong> Se você assina, guardamos seu e-mail
          e o registro do consentimento. A inscrição usa confirmação em duas etapas (double opt-in):
          o e-mail só é efetivado depois que você clica no link de confirmação. Base legal:
          consentimento. Você pode cancelar a qualquer momento pelo link de descadastro.
        </p>
        <p>
          <strong className="text-site-fg">Comentários.</strong> Ao comentar, guardamos o nome que
          você informa e o texto do comentário, que passa por moderação antes de aparecer. Para
          conter abuso, associamos ao comentário um <em>hash</em> (código irreversível) derivado do
          seu IP e navegador — <strong className="text-site-fg">o IP em si não é armazenado</strong>.
          Base legal: legítimo interesse na integridade da discussão.
        </p>
        <p>
          <strong className="text-site-fg">Reações.</strong> As curtidas são anônimas; usamos um
          identificador de visitante em cookie técnico apenas para evitar contagem duplicada.
        </p>
        <p>
          <strong className="text-site-fg">Logs de servidor.</strong> Como qualquer site, o servidor
          registra acessos (incluindo endereço IP e navegador) por tempo limitado, para segurança e
          diagnóstico. Base legal: legítimo interesse.
        </p>
      </Secao>

      <Secao titulo="O que NÃO fazemos">
        <p>
          Não usamos ferramentas de rastreamento ou analytics de terceiros, não exibimos anúncios e
          não vendemos nem compartilhamos seus dados para fins de marketing. Não há cookies de
          rastreamento — apenas os cookies técnicos necessários (sessão do administrador e
          identificador de visitante), sempre <code className="text-site-fg">httpOnly</code> quando
          aplicável.
        </p>
      </Secao>

      <Secao titulo="Por quanto tempo guardamos">
        <p>
          O e-mail da newsletter fica enquanto a inscrição estiver ativa; ao cancelar, é removido ou
          anonimizado. Comentários permanecem enquanto publicados. Logs de servidor têm retenção
          curta e são descartados no ciclo normal de operação.
        </p>
      </Secao>

      <Secao titulo="Seus direitos">
        <p>
          Pela LGPD, você pode a qualquer momento solicitar acesso, correção, exclusão ou
          portabilidade dos seus dados, além de revogar consentimentos. Basta escrever para{" "}
          <a href={`mailto:${EMAIL}`} className="text-site-acc underline-offset-2 hover:underline">
            {EMAIL}
          </a>{" "}
          — respondo diretamente.
        </p>
      </Secao>

      <Secao titulo="Mudanças nesta política">
        <p>
          Se algo mudar no tratamento de dados, esta página é atualizada com a nova data de vigência.
          Vale sempre a versão publicada aqui.
        </p>
      </Secao>
    </div>
  );
}
