// Conteudo estatico bilingue da home do site pessoal. Textos de UI (podem ter
// acento/cedilha, ao contrario de identificadores). O conteudo de artigo NAO
// vem daqui — vem da Content API e continua so em PT.

import type { Bilingue } from "@/lib/i18n/textos";

export type CardMetodo = {
  numero: string;
  titulo: Bilingue;
  descricao: Bilingue;
};

export const metodos: readonly CardMetodo[] = [
  {
    numero: "01",
    titulo: { pt: "Arquiteto no comando", en: "Architect in command" },
    descricao: {
      pt: "Eu desenho a arquitetura, defino os padrões e respondo pela qualidade. A IA executa o que eu reviso. Nada de vibe coding.",
      en: "I design the architecture, set the patterns and own the quality. AI executes what I review. No vibe coding.",
    },
  },
  {
    numero: "02",
    titulo: { pt: "Revisão adversarial", en: "Adversarial review" },
    descricao: {
      pt: "Todo plano passa por uma revisão que procura a falha antes dela virar código. É o passo que troca retrabalho por entrega.",
      en: "Every plan goes through a review that hunts the flaw before it becomes code. The step that trades rework for delivery.",
    },
  },
  {
    numero: "03",
    titulo: { pt: "Prova em runtime", en: "Proven at runtime" },
    descricao: {
      pt: "Código presente não é código correto. Cada entrega é provada rodando, com teste que tenta quebrar o sistema e não consegue.",
      en: "Code being there is not code being correct. Every delivery is proven running, with a test that tries to break it and fails.",
    },
  },
  {
    numero: "04",
    titulo: { pt: "Padrão sênior, custo enxuto", en: "Senior standard, lean cost" },
    descricao: {
      pt: "Clean Architecture, DDD, eventos e observabilidade. A disciplina de um squad sênior, sem o custo de um squad inteiro.",
      en: "Clean Architecture, DDD, events and observability. A senior squad's discipline, without a full squad's cost.",
    },
  },
];

export type CardCapacidade = {
  numero: string;
  icone: string;
  titulo: Bilingue;
  descricao: Bilingue;
};

export const capacidades: readonly CardCapacidade[] = [
  {
    numero: "01",
    icone: "⇄",
    titulo: { pt: "Arquitetura orientada a eventos", en: "Event-driven architecture" },
    descricao: {
      pt: "Outbox transacional, filas, idempotência e dead letter. Sistemas que não perdem mensagem nem duplicam operação.",
      en: "Transactional outbox, queues, idempotency and dead letter. Systems that don't lose messages or duplicate operations.",
    },
  },
  {
    numero: "02",
    icone: "▤",
    titulo: { pt: "Plataformas .NET e Azure", en: ".NET and Azure platforms" },
    descricao: {
      pt: "Microsserviços em .NET 6 a 9, Clean Architecture, DDD e gRPC, com observabilidade ponta a ponta.",
      en: "Microservices in .NET 6 to 9, Clean Architecture, DDD and gRPC, with end-to-end observability.",
    },
  },
  {
    numero: "03",
    icone: "◈",
    titulo: { pt: "Liderança técnica", en: "Technical leadership" },
    descricao: {
      pt: "Code review, mentoria e padronização. Elevo a senioridade do squad e a qualidade da entrega.",
      en: "Code review, mentoring and standardization. I raise the squad's seniority and the quality of delivery.",
    },
  },
  {
    numero: "04",
    icone: "⟲",
    titulo: { pt: "Modernização de legado", en: "Legacy modernization" },
    descricao: {
      pt: "Estrangulamento, contratos explícitos e medição. Tiro pedaços do monólito sem parar a operação.",
      en: "Strangler pattern, explicit contracts and measurement. I carve pieces off the monolith without stopping the operation.",
    },
  },
];

export type Experiencia = {
  empresa: string;
  periodo: Bilingue;
  papel: Bilingue;
};

export const experiencias: readonly Experiencia[] = [
  {
    empresa: "Avanade",
    periodo: { pt: "desde 2025", en: "since 2025" },
    papel: {
      pt: "Consultor Sênior / Tech Lead · billing global, Durable Functions, Azure",
      en: "Senior Consultant / Tech Lead · global billing, Durable Functions, Azure",
    },
  },
  {
    empresa: "Noordem",
    periodo: { pt: "2024 a 2025", en: "2024 to 2025" },
    papel: {
      pt: "Eng. Sênior / Tech Lead · cartão de crédito, microsserviços .NET, Kafka",
      en: "Senior Engineer / Tech Lead · credit card, .NET microservices, Kafka",
    },
  },
  {
    empresa: "Edenred",
    periodo: { pt: "2020 a 2024", en: "2020 to 2024" },
    papel: {
      pt: "Analista Dev Sênior · APIs .NET Core e Azure, Service Bus, Serilog",
      en: "Senior Dev Analyst · .NET Core and Azure APIs, Service Bus, Serilog",
    },
  },
  {
    empresa: "SIGCORP",
    periodo: { pt: "2019 a 2020", en: "2019 to 2020" },
    papel: {
      pt: "Gerente de TI · 22+ pessoas, modernização, Scrum, CI/CD, LGPD",
      en: "IT Manager · 22+ people, modernization, Scrum, CI/CD, LGPD",
    },
  },
  {
    empresa: "Grupo Talan",
    periodo: { pt: "2019 a 2020", en: "2019 to 2020" },
    papel: {
      pt: "Arquiteto / Sócio-fundador · fábrica de software, .NET, mobile",
      en: "Architect / Co-founder · software factory, .NET, mobile",
    },
  },
];

export type Fato = {
  chave: Bilingue;
  valor: Bilingue;
};

export const fatos: readonly Fato[] = [
  { chave: { pt: "base", en: "based" }, valor: { pt: "São Paulo, BR", en: "São Paulo, BR" } },
  { chave: { pt: "experiência", en: "experience" }, valor: { pt: "15+ anos", en: "15+ years" } },
  {
    chave: { pt: "foco", en: "focus" },
    valor: { pt: "arquitetura · entrega com IA", en: "architecture · AI delivery" },
  },
  { chave: { pt: "agora", en: "now" }, valor: { pt: "Avanade", en: "Avanade" } },
  {
    chave: { pt: "idiomas", en: "languages" },
    valor: { pt: "PT nativo · EN técnico", en: "PT native · EN technical" },
  },
];

export const pilhaTecnica: readonly string[] = [
  "C# / .NET",
  "Azure",
  "Kafka",
  "Service Bus",
  "DDD",
  "gRPC",
  "PostgreSQL",
  "Kubernetes",
];

/** Linhas de saida do terminal do hero (revelacao escalonada via CSS). */
export const terminalLinhas: readonly Bilingue[] = [
  { pt: "> Arquiteto de Software · Tech Lead", en: "> Software Architect · Tech Lead" },
  { pt: "> .NET 9 · Azure · DDD · event-driven", en: "> .NET 9 · Azure · DDD · event-driven" },
  { pt: "> 15 anos · bancário e financeiro", en: "> 15 years · banking and finance" },
  { pt: "> entrega: IA sob governança", en: "> delivery: AI under governance" },
  { pt: "> status: aberto a projetos", en: "> status: open to projects" },
];

export type RepoVitrine = {
  /** "owner/repo" — casa com GITHUB_SHOWCASE_REPOS. */
  chave: string;
  nome: string;
  descricao: Bilingue;
  linguagem: string;
  licenca?: string;
};

// Descricoes CURADAS (melhores que a do GitHub cru). Metadados ao vivo (estrelas,
// linguagem, licenca) enriquecem via lib/github quando ha token; sem token,
// caem nestes valores. TODO(GAP-K): confirmar os owners reais.
export const reposVitrine: readonly RepoVitrine[] = [
  {
    chave: "michel-az-de/levante",
    nome: "levante",
    descricao: {
      pt: "Engine de publicação headless em .NET. Move este site.",
      en: "Headless .NET publishing engine. Powers this site.",
    },
    linguagem: "C#",
    licenca: "Apache-2.0",
  },
  {
    chave: "felipeazevedoit/oracle-pack",
    nome: "oracle-pack",
    descricao: {
      pt: "Análise estática em Roslyn: risco, código morto e contexto para LLMs.",
      en: "Roslyn static analysis: risk, dead code and context for LLMs.",
    },
    linguagem: "C#",
  },
  {
    chave: "felipeazevedoit/easystock",
    nome: "easystock",
    descricao: {
      pt: "SaaS multi-tenant de estoque e produção. Construído com o loop da seção 01.",
      en: "Multi-tenant inventory and production SaaS. Built with the loop from section 01.",
    },
    linguagem: "C#",
  },
  {
    chave: "felipeazevedoit/hiram",
    nome: "hiram",
    descricao: {
      pt: "Notificações com outbox, RabbitMQ e dead letter.",
      en: "Notifications with outbox, RabbitMQ and dead letter.",
    },
    linguagem: "C#",
  },
];
