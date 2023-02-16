#!/bin/bash
Dockerfile="docker/server.Dockerfile"
DockerImage="dotnet-tests-source-code"
echo "Creating test image $DockerImage"
docker build -f $Dockerfile --target source-code -t $DockerImage . 
echo "Running container with tests"
# executando testes em um container
docker run --rm -i $DockerImage << EOF
cd /App
./dotnet-test.sh
EOF
DotnetTestsCode=$?
Result=0
if [ $DotnetTestsCode = 0 ]; then 
    echo "Succesfull tests"
else
    echo "Dotnet tests fail. Code: $DotnetTestsCode"
    Result=127
fi
docker rmi $DockerImage
exit $Result