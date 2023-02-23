#!/bin/bash
Dockerfile="webclient.Dockerfile"
Image="battlesim:webclient"
docker build . -f "$PWD/$Dockerfile" -t $Image
exit $?