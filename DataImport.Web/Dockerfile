#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0-bullseye-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Update packages
RUN apt-get update && apt-get upgrade -y && apt-get clean

FROM mcr.microsoft.com/dotnet/sdk:6.0-bullseye-slim AS build
WORKDIR /src
COPY ["DataImport.Web/DataImport.Web.csproj", "DataImport.Web/"]
COPY ["DataImport.Models/DataImport.Models.csproj", "DataImport.Models/"]
COPY ["DataImport.EdFi/DataImport.EdFi.csproj", "DataImport.EdFi/"]
COPY ["DataImport.Common/DataImport.Common.csproj", "DataImport.Common/"]
RUN dotnet restore "DataImport.Web/DataImport.Web.csproj"
COPY . .
WORKDIR "/src/DataImport.Web"
RUN dotnet build "DataImport.Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DataImport.Web.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "DataImport.Web.dll"]
