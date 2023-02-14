VolumeName="dotnet-test-vol"
docker volume inspect $VolumeName
if [ $? != 0 ]; then
    echo "Creating volume $VolumeName"
    docker volume create $VolumeName
fi
Mountpoint=`docker volume inspect --format '{{ .Mountpoint }}' $VolumeName`
echo "$VolumeName mountpoint: $Mountpoint"
ls -l $Mountpoint
echo "Replicating file between $PWD and $Mountpoint"
mkdir "$Mountpoint/app"
cp -r "$PWD/Server" "$Mountpoint/app"
cp -r "$PWD/Engine" "$Mountpoint/app"
cp "$PWD/dotnet-test.sh" "$Mountpoint/app/dotnet-test.sh"
echo "Volume $VolumeName is ready."
# executando testes em um container
docker run --rm -i -v $VolumeName:/opt mcr.microsoft.com/dotnet/sdk:7.0 << EOF
cd /opt/app
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
docker volume rm $VolumeName
exit $Result