FROM nginxinc/nginx-unprivileged:1.29.8-alpine

COPY nginx.conf /etc/nginx/conf.d/default.conf
COPY index.html app.js rsa.js /usr/share/nginx/html/
COPY img /usr/share/nginx/html/img/
