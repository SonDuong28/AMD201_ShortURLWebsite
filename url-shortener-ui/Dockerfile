# --- Build code Vue ---
FROM node:20-alpine AS build
WORKDIR /app

COPY package.json .
COPY package-lock.json .

RUN npm install

COPY . .

RUN npm run build

# --- Run Web Server ---
FROM nginx:1.25-alpine AS final

COPY --from=build /app/dist /usr/share/nginx/html

EXPOSE 80

CMD ["nginx", "-g", "daemon off;"]