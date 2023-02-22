#!/bin/bash
Dockerfile="webclient.Dockerfile"
Image="battlesim:webclient"
docker build . -f $Dockerfile -t $Image
exit $?