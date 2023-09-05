#!/bin/bash
EnvFilename=".env.local"
SecondsAuthTokenExpire=60
# if env variables are empty, exit
if [ -z $JwtSecret ]; then 
    echo "JwtSecret can not be empty"
    exit 127
fi
if [ -z $ApiUrl ]; then 
    echo "ApiUrl can not be empty"
    exit 127
fi
if [ -z $SiteUrl ]; then 
    echo "SiteUrl can not be empty"
    exit 127
fi
if [ -z $DbFileName ]; then
	DbFileName="/etc/game/gamedb.json"
fi
if [ -z $IntevalToMoveEntitiesInSeconds ]; then
	IntervalToMoveEntitiesInSeconds=1
fi
# write server configs in env file
echo -e "SecondsAuthTokenExpire=$SecondsAuthTokenExpire\nJwt:Issuer=$ApiUrl\nJwt:Secret=$JwtSecret\nJwt:Audience=$SiteUrl\nAllowedOrigin=$SiteUrl\nDbFileName=$DbFileName\nIntervalToMoveEntitiesInSeconds=$IntervalToMoveEntitiesInSeconds" > $EnvFilename
Imagename="battlesim:server"
ContainerName="battlesim-server"
LocalPort=3002
# stop the container if already is running
PreviousContainerId=`docker ps -a -f "name=$ContainerName" --format "{{ .ID }}"`
if [ -n "$PreviousContainerId" ]; then
    echo "Stoping container $PreviousContainerId"
    docker container stop $PreviousContainerId
    docker container rm $PreviousContainerId
    # removing old images used on older containers
    docker image prune -f
fi
GameDataVolume="battle-sim-data-vol"
docker run \
-dp $LocalPort:80 \
--name $ContainerName \
--env-file $EnvFilename \
-v $GameDataVolume:/etc/game \
$Imagename
exit $?
