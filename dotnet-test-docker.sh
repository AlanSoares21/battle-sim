#!/bin/bash
Dockerfile="docker/server.Dockerfile"
DockerImage="dotnet-tests-source-code"
echo "Creating test image $DockerImage"
docker build -f $Dockerfile --target source-code -t $DockerImage . 
echo "Image $Dockerfile builded."
# creating volume to store nuget packages
NugetPackagesVolumeName="nuget-packages-vol"
if docker volume list | grep $NugetPackagesVolumeName; then
    echo "Volume $NugetPackagesVolumeName is ready."
else
    echo "Creating volume $NugetPackagesVolumeName."
    docker volume create $NugetPackagesVolumeName
fi
echo "Running container with tests"
# executando testes em um container
NugetPackagesPath="/root/.nuget/packages"
docker run --rm -v $NugetPackagesVolumeName:$NugetPackagesPath -i $DockerImage << EOF
cd /App
./dotnet-test.sh
EOF
DotnetTestsCode=$?
Result=0
if [ $DotnetTestsCode -eq 0 ]; then 
    echo "Succesfull tests"
else
    echo "Dotnet tests fail. Code: $DotnetTestsCode"
    Result=127
fi
docker rmi $DockerImage
exit $Result