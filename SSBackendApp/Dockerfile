FROM mcr.microsoft.com/dotnet/core/aspnet:3.1 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
WORKDIR /src
COPY ["SSBackendApp.csproj","./"]
RUN dotnet restore "./SSBackendApp.csproj"
COPY . ./															
RUN dotnet build "SSBackendApp.csproj" -c Release -o /app

FROM build as publish
RUN dotnet publish "SSBackendApp.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet","SSBackendApp.dll"]