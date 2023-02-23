#!/bin/bash
SourceFolder="Clients/Web"
Dockerfile="webclient.Dockerfile"
Image="battlesim:webclient"
docker build $SourceFolder -f "$SourceFolder/$Dockerfile" -t $Image
exit $?