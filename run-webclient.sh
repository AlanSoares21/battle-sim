#!/bin/bash
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
StaticFilesVolume="battlesim-webclient-vol"
Image="battlesim:webclient"
PORT=3000
docker run \
-dp $PORT:80 \
--name $ContainerName \
-v $StaticFilesVolume:/usr/share/nginx/html \
$Image
exit $?
