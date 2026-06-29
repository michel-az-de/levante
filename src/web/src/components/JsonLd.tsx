// Renderiza um bloco JSON-LD (structured data) no HTML server-rendered.
// JSON.stringify NAO escapa "<" nem os separadores de linha U+2028/U+2029, entao
// um titulo/resumo contendo "</script>" (conteudo legitimo num blog tecnico, ou
// payload malicioso) fecharia a tag e injetaria markup. Escapamos na origem.
// Regex e prefixo "\u" montados via fromCharCode para o source ficar 100% ASCII.
const SEPARADORES_LINHA = String.fromCharCode(0x2028, 0x2029);
const CARACTERES_INSEGUROS = new RegExp("[<" + SEPARADORES_LINHA + "]", "g");
const PREFIXO_ESCAPE = String.fromCharCode(0x5c) + "u"; // "\u"

function escaparParaScript(json: string): string {
  return json.replace(
    CARACTERES_INSEGUROS,
    (c) => PREFIXO_ESCAPE + c.charCodeAt(0).toString(16).padStart(4, "0"),
  );
}

export function JsonLd({ data }: { data: object }) {
  return (
    <script
      type="application/ld+json"
      dangerouslySetInnerHTML={{ __html: escaparParaScript(JSON.stringify(data)) }}
    />
  );
}
