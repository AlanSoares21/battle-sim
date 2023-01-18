FROM nginx as PROD
COPY nginx-conf /etc/nginx/conf.d
COPY build /usr/share/nginx/html
EXPOSE 80