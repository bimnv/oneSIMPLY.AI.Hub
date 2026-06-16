FROM node:20-alpine AS build
WORKDIR /app
COPY Frontend/oneSIMPLY.AIHub.Web/package*.json ./
RUN npm ci
COPY Frontend/oneSIMPLY.AIHub.Web/ ./
RUN npm run build

FROM nginx:alpine AS runtime
COPY Frontend/oneSIMPLY.AIHub.Web/nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist /usr/share/nginx/html
EXPOSE 80
