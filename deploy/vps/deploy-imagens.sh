#!/usr/bin/env bash
# Deploy do Levante na VM (ADR 0009, stack em /opt/stacks/levante): fixa as imagens do GHCR de um
# commit e recria SO os servicos pedidos. Roda na VM, lido do stdin pelo ssh; e o mesmo bloco do
# job `deploy` do CI e do deploy manual:
#
#   ssh <vm> bash -s -- <sha-40> levante-web [levante-api] < deploy/vps/deploy-imagens.sh
#
# Ordem: valida tudo, puxa TODAS as imagens e so depois mexe nas tags e nos containers. Falha no
# pull nao deixa nenhuma tag trocada. O `compose up` roda sempre e decide pelo container que esta
# rodando, entao um rerun completa um deploy que caiu no meio e nao recria o que ja esta certo.
#
# Rollback: o primeiro deploy de um <sha> guarda a imagem anterior em <servico>:vps-pre-<sha7>
# (sha7 do deploy a desfazer; a tag nunca e sobrescrita):
#   docker tag <servico>:vps-pre-<sha7> <servico>:vps
#   cd /opt/stacks/levante && docker compose up -d --no-deps --pull never --wait <servico>
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
for servico in "$@"; do
  case "$servico" in
    levante-web | levante-api) ;;
    *)
      echo "servico desconhecido: $servico" >&2
      exit 2
      ;;
  esac
done

cd "$STACK"
docker compose config -q

# Um deploy por vez (job e manual nao se intercalam).
exec 9> "$STACK/.deploy.lock"
flock -w 600 9

for servico in "$@"; do
  docker pull -q "$REGISTRO/$servico:$SHA"
done

for servico in "$@"; do
  novo="$REGISTRO/$servico:$SHA"
  alvo=$(docker image inspect "$novo" --format '{{.Id}}')
  atual=$(docker image inspect "$servico:vps" --format '{{.Id}}' 2>/dev/null || true)
  backup="$servico:vps-pre-${SHA:0:7}"

  if [ -n "$atual" ] && [ "$atual" != "$alvo" ] \
    && ! docker image inspect "$backup" > /dev/null 2>&1; then
    docker tag "$servico:vps" "$backup"
    echo "$servico: imagem anterior guardada em $backup"
  fi
  docker tag "$novo" "$servico:vps"
done

docker compose up -d --no-deps --pull never --wait --wait-timeout 180 "$@"
docker compose ps "$@"
