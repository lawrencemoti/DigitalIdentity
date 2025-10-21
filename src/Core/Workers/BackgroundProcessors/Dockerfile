# BackgroundProcessors/Dockerfile
FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src/Core/Workers
COPY . .
RUN dotnet restore BackgroundProcessors/BackgroundProcessors.csproj && dotnet publish Worker/Worker.csproj -c Release -o /out

FROM base AS final
WORKDIR /app
COPY --from=build /out .
ENTRYPOINT ["dotnet", "BackgroundProcessors.dll"]
