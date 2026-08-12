# ADR 0009 — Hospedagem: VPS próprio com stacks separadas, Hiram alcançado in-network

Status: **Aceito** · Fatia D (lançamento) · 2026-08-12 · **supersede parcialmente o [ADR 0003](0003-hospedagem-vm-conjunta-hiram.md)**

## Contexto

O ADR 0003 decidiu hospedar Levante e Hiram na mesma VM, via o Compose conjunto de
`hiram/deploy/stack/`. Duas premissas daquele documento deixaram de valer, e as duas foram medidas em
2026-08-12, contra o repositório e não contra documentação.

**O deploy conjunto saiu do produto Hiram.** O `ADR-027: Hiram Core como infraestrutura interna
enxuta`, aceito em 2026-07-29, lista sob "Saem do produto", entre outros itens, o **deploy conjunto
com Levante**. O mesmo ADR reduz o runtime do Hiram a um host único com PostgreSQL como única peça
obrigatória de estado, e retira RabbitMQ e a stack LGTM da fronteira do projeto.

**A stack que o ADR 0003 cita como pronta e revisada não existe mais.** O 0003 se apoia em
"já existe uma stack conjunta pronta e revisada no repo do Hiram (`hiram/deploy/stack/`, PR #24)".
Hoje, por `git ls-files`, aquele diretório versiona apenas `.env.example` e `.gitignore`, e
`deploy/levante/` apenas `.gitignore`. Não há `docker-compose.yml`, `README.md` de bring-up,
`provision-levante.sh` nem `deploy-app.sh`. A migração do ADR-027 retirou esses artefatos no passo
que ele chama de "retirar escopos de produto e deploys fora da fronteira".

Escrever o Compose conjunto agora seria implementar no repositório do Hiram exatamente aquilo que um
ADR aceito removeu de lá. Não fazer nada mantém o Levante sem hospedagem enquanto há VPS próprio
contratado, com Caddy e PostgreSQL 17 compartilhados já de pé, e enquanto o motivo da migração é
justamente custo de nuvem.

**O eixo da reconciliação é que "mesma VM" e "deploy conjunto" não são a mesma coisa.** O ADR 0003
quis co-locação, por custo, com o Hiram sem superfície pública e alcançado in-network. Nada disso
exige um artefato de Compose único mantido no repositório do Hiram. O que o ADR-027 recusa é o
artefato compartilhado, não a co-locação.

## Decisões

1. **Levante e Hiram continuam na mesma máquina, em stacks Compose separadas.** Cada produto tem seu
   próprio diretório e seu próprio `docker-compose.yml`, sob `/opt/stacks/`. Nenhum repositório
   vendoriza o deploy do outro, e nenhum artefato de deploy conjunto é criado.

2. **O Hiram continua sem superfície pública, e o Levante o alcança in-network.** Os dois compartilham
   uma rede Docker externa. A chamada segue sendo HTTP com `X-Api-Key`, exatamente o contrato do
   [ADR 0002](0002-emissao-hiram-http.md), que este documento não toca. Nenhuma porta do Hiram é
   publicada no host.

3. **O Caddy segue como única superfície pública**, nas portas 80 e 443, com Let's Encrypt
   automático. Ele deixa de pertencer à stack conjunta e passa a viver numa stack de infraestrutura
   compartilhada, junto do PostgreSQL. É a mesma decisão do 0003 com outro dono.

4. **MongoDB passa a ser self-hosted na própria máquina**, substituindo o Atlas externo da decisão 2
   do ADR 0003. O motivo é o mesmo que motivou toda a consolidação: o Atlas é banco gerenciado pago, e
   o 0003 já reconhecia que co-hospedar "não elimina o custo de banco gerenciado". A regra de
   privilégio mínimo do usuário de runtime **continua valendo integralmente**: o self-check de boot
   aborta em Produção se o usuário tiver role administrativa, e o teste do gate `polish` continua de
   pé. Muda onde o banco roda, não quem pode o quê dentro dele.

5. **`mongodump` agendado deixa de ser opcional e vira requisito de go-live.** Sem o Atlas some o
   backup gerenciado e o PITR. Como a mídia de artigo mora no GridFS do próprio banco
   ([ADR 0008](0008-midia-gridfs.md)), perder o volume perde também as imagens, que não são
   recuperáveis do markdown, já que o corpo guarda apenas `/midias/{id}`. O dump arrasta binários,
   então a janela de backup e restore precisa ser medida com eles.

6. **O portão D0.5 do runbook continua vinculante**, sem alteração: backup e restore do keyring de
   Data Protection do Hiram, testados, antes de qualquer coisa pública.

## O que do ADR 0003 continua valendo

Este documento é cirúrgico de propósito. Continuam válidas, sem alteração:

- co-locação numa máquina só, e o custo como razão dela;
- Caddy como única superfície pública, Hiram nunca exposto, chamada in-network;
- imagens publicadas pelo CI de cada repositório, produção fixando `<sha>` e nunca `latest`;
- CD escopado por serviço, recriando só os serviços do produto que mudou;
- segredos em `.env` na máquina, `chmod 600`, dono igual ao usuário de deploy;
- rollback manual em par API e web, por SHA;
- **o gatilho de reversão**: esta decisão vale enquanto o Hiram não tiver tenants externos reais com
  expectativa de uptime. Se tiver, o raio de explosão compartilhado e a custódia do keyring numa
  máquina de portfólio deixam de ser aceitáveis, e o Levante sai para máquina própria.

São superseding apenas: o Compose conjunto de `hiram/deploy/stack/` como veículo (decisão 1), o
MongoDB Atlas (decisão 2) e a observabilidade via `otel-lgtm` acoplada ao deploy, que o ADR-027
retirou da fronteira do Hiram. O aplicativo segue OTLP-native, então religar um coletor depois não
exige mudança de código.

## Consequências

Positivas:

- nenhum artefato contradiz ADR aceito, em nenhum dos dois repositórios;
- o Hiram fica livre para evoluir dentro da fronteira do ADR-027 sem arrastar o Levante junto;
- o PostgreSQL compartilhado que já está de pé serve o Hiram sem container novo, e o ADR-027 pede
  exatamente PostgreSQL como única peça de estado;
- some o custo do Atlas, que era o último banco gerenciado pago do conjunto.

Negativas e mitigações:

- **Backup vira responsabilidade explícita e integral.** Mitigação: decisão 5, com `mongodump`
  agendado como requisito de go-live e não como intenção.
- **Duas stacks significam duas janelas de recriação** em vez de uma. Mitigação: já era o desenho do
  CD escopado por serviço do ADR 0003, que este documento preserva; na prática muda pouco.
- **A rede compartilhada é agora a fronteira de confiança entre os produtos.** Mitigação: o Hiram
  continua sem porta publicada, e a autenticação por `X-Api-Key` do ADR 0002 não afrouxa por os dois
  estarem na mesma rede.
- **O runbook de lançamento fica desatualizado neste ponto**, e continua desatualizado quanto a
  RabbitMQ, dispatcher separado e Grafana/Loki. Mitigação: issue própria de reconciliação, aberta
  junto com este ADR. Reescrever o runbook aqui dobraria o tamanho da entrega e atrasaria a decisão.

## Alternativas descartadas

- **Escrever o Compose conjunto no repositório do Hiram.** É o caminho literal do ADR 0003, e foi o
  pedido original. Descartado porque implementaria no Hiram exatamente o item que o ADR-027 aceito
  removeu, e nenhum dos dois repositórios permite contrariar ADR sem ADR novo.
- **Abrir ADR no Hiram revertendo aquele item do ADR-027.** Legítimo, e continua disponível. Descartado
  porque reabre uma decisão de fronteira de produto, tomada há duas semanas com contexto próprio, para
  resolver um problema de hospedagem que não precisa dela.
- **Vendorizar a stack do Hiram dentro do repositório do Levante.** Elimina a dependência de um
  artefato que não existe. Descartado porque cria a segunda fonte de verdade que o ADR 0003 recusou
  explicitamente ao dizer que a stack tem "fonte única no repo Hiram".
- **Manter o MongoDB no Atlas e mudar só o veículo de deploy.** Menor mudança, e preserva backup
  gerenciado. Descartado porque deixa de pé o custo recorrente que motivou a consolidação inteira, e o
  próprio ADR 0003 já registrava que a co-locação não o elimina.
