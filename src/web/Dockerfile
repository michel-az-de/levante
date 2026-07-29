# syntax=docker/dockerfile:1.7
# Build from src/web: docker build -f src/web/Dockerfile src/web
# Next.js standalone (requer output: "standalone" no next.config.ts). Node 20 (engines >= 20).
FROM node:20-alpine AS deps
WORKDIR /app
COPY package.json package-lock.json ./
RUN npm ci

FROM node:20-alpine AS build
WORKDIR /app
COPY --from=deps /app/node_modules ./node_modules
COPY . .
# next build consome os tipos gerados do OpenAPI ja commitados; nao precisa da API viva.
RUN npm run build

FROM node:20-alpine AS runtime
WORKDIR /app
ENV NODE_ENV=production \
    PORT=3000
RUN addgroup -S nodejs && adduser -S nextjs -G nodejs
# --chown obrigatorio: sem ele o COPY entra como root e o servidor, ja como nextjs,
# nao consegue criar /app/.next/cache (EACCES a cada request que grava cache de
# ISR/fetch). O mkdir explicito garante o diretorio mesmo quando nada em .next/static
# o tenha criado antes.
COPY --chown=nextjs:nodejs --from=build /app/.next/standalone ./
COPY --chown=nextjs:nodejs --from=build /app/.next/static ./.next/static
RUN mkdir -p /app/.next/cache && chown -R nextjs:nodejs /app/.next
USER nextjs
EXPOSE 3000
CMD ["node", "server.js"]
