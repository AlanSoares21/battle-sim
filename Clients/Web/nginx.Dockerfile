FROM nginx AS run
COPY ./nginx-conf/ /etc/nginx/conf.d
EXPOSE 80