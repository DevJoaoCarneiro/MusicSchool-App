# syntax=docker/dockerfile:1

# ---------- build ----------
FROM node:22-alpine AS build
WORKDIR /app

COPY package.json package-lock.json ./
RUN npm ci --no-audit --no-fund

COPY . .

ARG VITE_API_URL
ENV VITE_API_URL=${VITE_API_URL}
RUN test -n "$VITE_API_URL" || (echo "VITE_API_URL precisa ser informada no build (URL pública da API)." && exit 1)
RUN npm run build

# ---------- runtime ----------
FROM nginxinc/nginx-unprivileged:1.28-alpine AS runtime

ENV PORT=8080 \
    NGINX_ENTRYPOINT_WORKER_PROCESSES_AUTOTUNE=1

COPY nginx.conf.template /etc/nginx/templates/default.conf.template
COPY --from=build /app/dist /usr/share/nginx/html

EXPOSE 8080
