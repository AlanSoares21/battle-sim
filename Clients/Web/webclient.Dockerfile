FROM node
WORKDIR /code
COPY . .
EXPOSE 80
ENTRYPOINT [ "/code/run-server.sh" ]