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
COPY --from=build /app/.next/standalone ./
COPY --from=build /app/.next/static ./.next/static
USER nextjs
EXPOSE 3000
CMD ["node", "server.js"]
