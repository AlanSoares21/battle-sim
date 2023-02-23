FROM mcr.microsoft.com/dotnet/sdk:7.0 AS source-code
WORKDIR /App
COPY . ./

FROM source-code AS build-env
WORKDIR /App/Server/Core
RUN dotnet restore
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:7.0
WORKDIR /App
COPY --from=build-env /App/Server/Core/out .
EXPOSE 80
ENTRYPOINT ["dotnet", "BattleSimulator.Server.dll"]