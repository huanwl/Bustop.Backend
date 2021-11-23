FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["MapBus/MapBus.csproj", "MapBus/"]
RUN dotnet restore "MapBus/MapBus.csproj"
COPY . .
WORKDIR "/src/MapBus"
RUN dotnet build "MapBus.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MapBus.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
CMD dotnet MapBus.dll --urls http://+:$PORT