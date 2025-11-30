FROM node:20 AS build
WORKDIR /app
COPY package*.json ./
RUN npm install
COPY . .
RUN npm run build --prod

FROM nginx:alpine

# Copia i file della build (questo lo avevi già corretto)
COPY --from=build /app/dist/distributed-chess/browser /usr/share/nginx/html

# Sovrascrive la configurazione di default di Nginx con la tua
COPY nginx.conf /etc/nginx/conf.d/default.conf

EXPOSE 80