FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
VOLUME ["/app/images", "/app/actions"]
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app
ARG BUILD_CONFIGURATION=Release
COPY src/*.csproj ./src/
COPY bin/ ./bin/
COPY *.sln .
RUN dotnet restore
COPY src/ ./src/
RUN dotnet publish -c $BUILD_CONFIGURATION -o out

FROM base AS final
WORKDIR /app
COPY --from=build /app/out ./
ENTRYPOINT ["dotnet", "NAIBot.dll"]