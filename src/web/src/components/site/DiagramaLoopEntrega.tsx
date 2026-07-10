import type { SVGProps } from "react";

// Rotulo bilingue dentro do SVG: renderiza os dois <text> e o CSS [data-idioma]
// esconde o que nao vale (mesmo mecanismo do componente <Idioma>).
function TextoSvg({ pt, en, ...resto }: { pt: string; en: string } & SVGProps<SVGTextElement>) {
  return (
    <>
      <text data-idioma-pt="" {...resto}>
        {pt}
      </text>
      <text data-idioma-en="" {...resto}>
        {en}
      </text>
    </>
  );
}

type No = {
  x: number;
  w: number;
  linhas: readonly { pt: string; en: string }[];
  destaque?: boolean;
};

const NOS: readonly No[] = [
  { x: 8, w: 104, linhas: [{ pt: "Arquiteto", en: "Architect" }] },
  { x: 140, w: 100, linhas: [{ pt: "Plano", en: "Plan" }, { pt: "(IA)", en: "(AI)" }] },
  { x: 268, w: 132, linhas: [{ pt: "Revisão", en: "Review" }, { pt: "adversarial", en: "adversarial" }] },
  { x: 428, w: 104, linhas: [{ pt: "Código", en: "Code" }, { pt: "(IA)", en: "(AI)" }] },
  { x: 560, w: 112, linhas: [{ pt: "Prova em", en: "Runtime" }, { pt: "runtime", en: "proof" }] },
  { x: 700, w: 72, linhas: [{ pt: "Entrega", en: "Ship" }], destaque: true },
];

// Setas: da borda direita de um no para a esquerda do proximo, em y=100.
const SETAS: readonly { de: number; para: number }[] = [
  { de: 112, para: 140 },
  { de: 240, para: 268 },
  { de: 400, para: 428 },
  { de: 532, para: 560 },
  { de: 672, para: 700 },
];

const COR_TX = "var(--color-site-fg)";
const COR_AR = "var(--color-site-fg2)";
const COR_LB = "var(--color-site-faint)";
const COR_ACC = "var(--color-site-acc)";

/** Diagrama SVG do loop de entrega (arquiteto define/revisa/prova; IA executa). */
export function DiagramaLoopEntrega() {
  return (
    <svg
      viewBox="0 0 780 200"
      className="mx-auto block h-auto w-full max-w-[720px] font-site-mono"
      role="img"
      aria-label="Loop de entrega: arquiteto, plano, revisao adversarial, codigo, prova em runtime, entrega"
    >
      {NOS.map((no) => {
        const cx = no.x + no.w / 2;
        return (
          <g key={no.x}>
            <rect
              x={no.x}
              y={80}
              width={no.w}
              height={40}
              fill={no.destaque ? "color-mix(in srgb, var(--color-site-acc) 12%, var(--color-site-bg2))" : "var(--color-site-bg2)"}
              stroke={no.destaque ? COR_ACC : "var(--color-site-line2)"}
            />
            {no.linhas.length === 1 ? (
              <TextoSvg
                pt={no.linhas[0].pt}
                en={no.linhas[0].en}
                x={cx}
                y={104}
                textAnchor="middle"
                fill={COR_TX}
                fontSize={12}
              />
            ) : (
              <>
                <TextoSvg
                  pt={no.linhas[0].pt}
                  en={no.linhas[0].en}
                  x={cx}
                  y={97}
                  textAnchor="middle"
                  fill={COR_TX}
                  fontSize={12}
                />
                <TextoSvg
                  pt={no.linhas[1].pt}
                  en={no.linhas[1].en}
                  x={cx}
                  y={111}
                  textAnchor="middle"
                  fill={COR_TX}
                  fontSize={12}
                />
              </>
            )}
          </g>
        );
      })}

      {SETAS.map((seta) => (
        <g key={seta.de}>
          <path d={`M${seta.de} 100 L${seta.para} 100`} stroke={COR_AR} strokeWidth={1.4} fill="none" />
          <path d={`M${seta.para} 100 l-7 -3.5 v7 z`} fill={COR_AR} />
        </g>
      ))}

      {/* Feedback: revisao acha gap -> volta pro plano. */}
      <path d="M334 80 C 334 38, 190 38, 190 80" stroke={COR_ACC} strokeWidth={1} strokeDasharray="3 3" fill="none" opacity={0.7} />
      <path d="M190 80 l-4 -7 h8 z" fill={COR_AR} />
      <TextoSvg pt="gaps P0 · P1" en="gaps P0 · P1" x={262} y={34} textAnchor="middle" fill={COR_LB} fontSize={10} />

      {/* Feedback: prova reprova -> volta pro codigo. */}
      <path d="M616 120 C 616 166, 480 166, 480 120" stroke={COR_ACC} strokeWidth={1} strokeDasharray="3 3" fill="none" opacity={0.7} />
      <path d="M480 120 l-4 7 h8 z" fill={COR_AR} />
      <TextoSvg pt="reprovou" en="failed" x={548} y={182} textAnchor="middle" fill={COR_LB} fontSize={10} />
    </svg>
  );
}
