ServerImageName="alan26silva/battlesim:server"
docker build . -t $ServerImageName -f ./docker/server.Dockerfile
exit $?