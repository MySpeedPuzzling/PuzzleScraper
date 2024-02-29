FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["./", "./"]
RUN dotnet restore "./Arctic.Puzzlers.Webapi/Arctic.Puzzlers.Webapi.csproj"
COPY . .
WORKDIR "/src"
RUN dotnet build "./Arctic.Puzzlers.Webapi/Arctic.Puzzlers.Webapi.csproj" -c $BUILD_CONFIGURATION -o /app/build


FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Arctic.Puzzlers.Webapi/Arctic.Puzzlers.Webapi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Arctic.Puzzlers.Webapi.dll"]