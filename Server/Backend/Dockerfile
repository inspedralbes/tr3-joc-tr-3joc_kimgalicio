# Utilitzem una imatge lleugera de Node.js
FROM node:18-alpine

# Creem el directori de treball
WORKDIR /app

# Copiem el package.json i package-lock.json primer (millora el cache de capes)
COPY package*.json ./

# Instal·lem les dependències
RUN npm install --production

# Copiem la resta del codi font
COPY . .

# Exposem el port que utilitza l'aplicació
EXPOSE 3000

# Comanda per defecte per iniciar el servidor
CMD ["node", "server.js"]
