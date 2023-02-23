#!/bin/bash
Dockerfile="webclient.Dockerfile"
Image="battlesim:webclient"
docker build . -f "$PWD/$Dockerfile" -t $Image
Result=$?
echo "Build result: $Result"
exit $Result