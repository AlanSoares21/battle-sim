#!/bin/bash
# checking env
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
# writing env file
EnvFile=".env.production.local"
SourceFolder="Clients/Web"
echo -e "REACT_APP_ServerWsUrl=$WsUrl\nREACT_APP_ServerApiUrl=$ApiUrl" | tee "$SourceFolder/$EnvFile"

# removing old containers
ContainerName="battlesimwebclient"
PreviousContainerId=`docker ps -a -f "name=$ContainerName" --format "{{ .ID }}"`
if [ -n "$PreviousContainerId" ]; then
    echo "Stoping container $PreviousContainerId"
    docker container stop $PreviousContainerId
    docker container rm $PreviousContainerId
    # removing old images used on older containers
    docker image prune -f
fi

# running container
NodeModulesVolume="battlesimwebclient-node-modules-vol"
Image="battlesim:webclient"
PORT=3000
docker run \
-dp $PORT:3000 \
--name $ContainerName \
--env-file "$SourceFolder/$EnvFile" \
-v $NodeModulesVolume:/code/node_modules \
$Image
exit $?