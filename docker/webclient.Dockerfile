FROM node as build
ARG WsSocketUrl
ARG ApiUrl
WORKDIR /code
COPY . .
RUN npm install --all
ENV REACT_APP_ServerWsUrl $WsSocketUrl
ENV REACT_APP_ServerApiUrl $ApiUrl
RUN npm run build

FROM nginx as PROD
COPY --from=build /code/nginx-conf /etc/nginx/conf.d
COPY --from=build /code/build /usr/share/nginx/html
EXPOSE 80