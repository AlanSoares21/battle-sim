Dockerfile="./docker/webclient.Dockerfile"
WebClientImageName="alan26silva/battlesim:webclient"
WsSocketUrl=$1
ApiUrl=$2
if [ -z $WsSocketUrl ]; then 
    echo "WebSocket URL can not be empty"
    exit 127
fi
if [ -z $ApiUrl ]; then 
    echo "ApiUrl URL can not be empty"
    exit 127
fi
# building image
docker build ./Clients/Web -t $WebClientImageName -f $Dockerfile --build-arg WsSocketUrl=$WsSocketUrl --build-arg ApiUrl=$ApiUrl
exit $?