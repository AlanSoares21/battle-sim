FROM node:18-alpine3.18
WORKDIR /code
COPY . .
ENTRYPOINT [ "sh", "/code/build.sh" ]