/** Marca (logotipo hexagonal) do site pessoal. Usa o acento do tema via CSS var. */
export function Marca({ className }: { className?: string }) {
  return (
    <svg viewBox="0 0 24 26" className={className} aria-hidden="true">
      <path
        d="M12 2 L22 8 L22 18 L12 24 L2 18 L2 8 Z"
        fill="none"
        stroke="var(--color-site-acc)"
        strokeWidth="1.5"
      />
      <path
        d="M12 2 L12 14 L22 8"
        fill="none"
        stroke="var(--color-site-acc)"
        strokeWidth="1"
        opacity="0.7"
      />
      <path
        d="M12 14 L2 8"
        fill="none"
        stroke="var(--color-site-acc)"
        strokeWidth="1"
        opacity="0.7"
      />
      <path d="M2 8 L12 14 L12 24 L2 18 Z" fill="var(--color-site-acc)" opacity="0.14" />
    </svg>
  );
}
