FROM mcr.microsoft.com/dotnet/sdk:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
ARG servicename
WORKDIR /src
COPY . .
RUN dotnet restore
WORKDIR "/src/src/$servicename"
RUN dotnet build -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
WORKDIR "/src/src/$servicename"
RUN dotnet publish -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app

RUN dotnet tool install --global dotnet-trace
# Add the .NET tools to the PATH
ENV PATH="$PATH:/root/.dotnet/tools"
COPY --from=publish /app/publish .
