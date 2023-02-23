#!/bin/bash
ServerImageName="battlesim:server"
docker build . -t $ServerImageName -f ./docker/server.Dockerfile
exit $?