Imagename="alan26silva/battlesim:webclient"
ContainerName="battlesim-webclient"
LocalPort=3003
# stop the container if already is running
PreviousContainerId=`docker ps -a -f "name=$ContainerName" --format "{{ .ID }}"`
if [ -n $PreviousContainerId ]; then
    echo "Stoping container $PreviousContainerId"
    docker container stop $PreviousContainerId
    docker container rm $PreviousContainerId
    # removing old images used on older containers
    docker image prune -f
fi
docker run \
-dp $LocalPort:80 \
--name $ContainerName \
$Imagename
exit $?