#!/bin/bash
if [ -z $AssetsUrl ]; then
	echo "You should provide the url to download the assets"
	exit 1;
fi
if [ -z $ApiUrl ]; then
	echo "You should provide the api url"
	exit 1;
fi
if [ -z $WsUrl ]; then
	echo "You should provide the web socket server url"
	exit 1;
fi

SourceFolder="Clients/Web"
EnvFileName="$SourceFolder/.env.production.local"
echo -e "AssetsUrl=$AssetsUrl\nREACT_APP_ServerWsUrl=$WsUrl\nREACT_APP_ServerApiUrl=$ApiUrl" > $EnvFileName

# Creating assets
PythonLibsVolume="build-assets-python-libs-vol"
BuildAssetsImage="build-client-assets"
BuildAssetsDockerfile="buildAssets.Dockerfile"
AssetsVolume="client-assets-vol"

echo "Starting build assets"
docker build -f "$SourceFolder/$BuildAssetsDockerfile" -t $BuildAssetsImage $SourceFolder

docker run --rm -v $PythonLibsVolume:/usr/local/lib -v $AssetsVolume:/public/assets --env-file $EnvFileName  -i $BuildAssetsImage 

# Building webclient files
BuildClientDockerfile="buildWebClient.Dockerfile"
BuildClientImage="battlesim-client-build"
NodeModulesVolume="battlesim-node-modules-vol"
ClientStaticFilesVolume="battlesim-webclient-vol"

echo "Starting build the client"
docker build $SourceFolder -f "$SourceFolder/$BuildClientDockerfile" -t $BuildClientImage

docker run --rm -v $NodeModulesVolume:/code/node_modules -v $AssetsVolume:/code/public/assets -v $ClientStaticFilesVolume:/code/build --env-file $EnvFileName --env CodeSrc="/code" -i $BuildClientImage

NginxImage="battlesim:webclient"
NginxDockerfile="nginx.Dockerfile"

docker build $SourceFolder -f "$SourceFolder/$NginxDockerfile" -t $NginxImage

exit $?
