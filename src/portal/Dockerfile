FROM node:20-slim
WORKDIR /app

COPY package.json package-lock.json ./
RUN npm install

COPY . .

EXPOSE 5173

# Vite only exposes VITE_-prefixed vars from a .env file, so write the ones
# passed by Compose into .env before starting the dev server.
CMD ["sh", "-c", "env | grep '^VITE_' > .env; npm run dev -- --host 0.0.0.0 --port 5173"]
