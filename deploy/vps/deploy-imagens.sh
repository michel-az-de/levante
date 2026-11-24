#!/usr/bin/env bash
# Deploy do Levante na VM (ADR 0009, stack em /opt/stacks/levante): fixa as imagens do GHCR de um
# commit e recria SO os servicos pedidos. Roda na VM, lido do stdin pelo ssh; e o mesmo bloco do
# job `deploy` do CI e do deploy manual:
#
#   ssh <vm> bash -s -- <sha-40> levante-web [levante-api] < deploy/vps/deploy-imagens.sh
#
# Rollback: cada servico trocado guarda a imagem anterior em <servico>:vps-pre-<sha7 novo>.
#   docker tag <servico>:vps-pre-<sha7> <servico>:vps
#   cd /opt/stacks/levante && docker compose up -d --no-deps --pull never <servico>
# Idempotente: servico que ja roda a imagem do <sha> nao e recriado.
set -euo pipefail

SHA="${1:-}"
shift || true
STACK="${LEVANTE_STACK_DIR:-/opt/stacks/levante}"
REGISTRO="ghcr.io/michel-az-de"

if [[ ! "$SHA" =~ ^[0-9a-f]{40}$ ]]; then
  echo "sha invalido: '$SHA' (esperado o sha completo, 40 hex)" >&2
  exit 2
fi
if [ "$#" -eq 0 ]; then
  echo "informe ao menos um servico: levante-web, levante-api" >&2
  exit 2
fi

trocados=()
for servico in "$@"; do
  case "$servico" in
    levante-web | levante-api) ;;
    *)
      echo "servico desconhecido: $servico" >&2
      exit 2
      ;;
  esac

  novo="$REGISTRO/$servico:$SHA"
  docker pull -q "$novo"
  alvo=$(docker image inspect "$novo" --format '{{.Id}}')
  atual=$(docker image inspect "$servico:vps" --format '{{.Id}}' 2>/dev/null || true)

  if [ "$atual" = "$alvo" ]; then
    echo "$servico ja esta em ${SHA:0:7}; nada a fazer"
    continue
  fi
  if [ -n "$atual" ]; then
    docker tag "$servico:vps" "$servico:vps-pre-${SHA:0:7}"
    echo "$servico: imagem anterior guardada em $servico:vps-pre-${SHA:0:7}"
  fi
  docker tag "$novo" "$servico:vps"
  trocados+=("$servico")
done

if [ "${#trocados[@]}" -eq 0 ]; then
  exit 0
fi

cd "$STACK"
docker compose up -d --no-deps --pull never "${trocados[@]}"
docker compose ps "${trocados[@]}"
