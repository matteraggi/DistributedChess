FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY DistributedChess.sln ./
COPY LobbyService/LobbyService.csproj LobbyService/
COPY Shared/Shared.csproj Shared/
COPY GameEngine/GameEngine.csproj GameEngine/

RUN dotnet restore DistributedChess.sln

COPY . .
RUN dotnet publish LobbyService/LobbyService.csproj -c Release -o /app/publish


# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 5000
ENTRYPOINT ["dotnet", "LobbyService.dll"]