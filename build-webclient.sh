#!/bin/bash
Dockerfile="./docker/webclient.Dockerfile"
WebClientImageName="alan26silva/battlesim:webclient"
# if empty, fill with cli arguments
if [ -z $WsUrl ]; then 
    WsUrl=$1
fi
if [ -z $ApiUrl ]; then 
    ApiUrl=$2
fi
# if still empty, exit
if [ -z $WsUrl ]; then 
    echo "WsUrl URL can not be empty"
    exit 127
fi
if [ -z $ApiUrl ]; then 
    echo "ApiUrl URL can not be empty"
    exit 127
fi
echo "WsUrl: $WsUrl"
echo "ApiUrl: $ApiUrl"
# building image
docker build ./Clients/Web -t $WebClientImageName -f $Dockerfile --build-arg WsUrl=$WsUrl --build-arg ApiUrl=$ApiUrl
exit $?