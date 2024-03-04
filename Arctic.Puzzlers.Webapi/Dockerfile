FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8081
EXPOSE 8080


FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Arctic.Puzzlers.Webapi/Arctic.Puzzlers.Webapi.csproj", "Arctic.Puzzlers.Webapi/"]
COPY ["Arctic.Puzzlers.Objects/Arctic.Puzzlers.Objects.csproj", "Arctic.Puzzlers.Objects/"]
COPY ["Arctic.Puzzlers.Parsers/Arctic.Puzzlers.Parsers.csproj", "Arctic.Puzzlers.Parsers/"]
COPY ["Arctic.Puzzlers.Stores/Arctic.Puzzlers.Stores.csproj", "Arctic.Puzzlers.Stores/"]
RUN dotnet restore "./Arctic.Puzzlers.Webapi/./Arctic.Puzzlers.Webapi.csproj"
COPY . .
WORKDIR "/src/Arctic.Puzzlers.Webapi"
RUN dotnet build "./Arctic.Puzzlers.Webapi.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Arctic.Puzzlers.Webapi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Arctic.Puzzlers.Webapi.dll"]
