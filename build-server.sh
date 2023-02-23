#!/bin/bash
ServerImageName="battlesim:server"
docker build . -t $ServerImageName -f ./docker/server.Dockerfile
Result=$?
echo "Build result: $Result"
exit $Result