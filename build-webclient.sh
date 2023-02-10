WebClientImageName="alan26silva/battlesim:webclient"
docker build ./Clients/Web -t $WebClientImageName -f ./docker/webclient.Dockerfile
exit $?