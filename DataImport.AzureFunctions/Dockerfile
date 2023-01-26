#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/azure-functions/dotnet-isolated:4-dotnet-isolated7.0 AS base
WORKDIR /home/site/wwwroot
EXPOSE 80

FROM mcr.microsoft.com/dotnet/runtime:7.0 as runtime7.0
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
# Copy .NET Core 7.0 runtime from the 7.0 image
COPY --from=runtime7.0 /usr/share/dotnet/host /usr/share/dotnet/host
COPY --from=runtime7.0 /usr/share/dotnet/shared /usr/share/dotnet/shared

RUN apt-get update && apt-get install -y unzip

WORKDIR /src
COPY ["DataImport.AzureFunctions/DataImport.AzureFunctions.csproj", "DataImport.AzureFunctions/"]
RUN dotnet restore "DataImport.AzureFunctions/DataImport.AzureFunctions.csproj"
COPY . .
WORKDIR "/src/DataImport.AzureFunctions"
RUN dotnet build "DataImport.AzureFunctions.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DataImport.AzureFunctions.csproj" -c Release -o /app/publish /p:UseAppHost=false
RUN unzip /src/DataImport.AzureFunctions/TransformLoadTool/DataImport.TransformLoad.zip -d /app/publish/TransformLoadTool

FROM base AS final
WORKDIR /home/site/wwwroot
COPY --from=publish /app/publish .

RUN chmod +x /home/site/wwwroot/TransformLoadTool/DataImport.Server.TransformLoad
RUN ln -sf /home/site/wwwroot/TransformLoadTool/DataImport.Server.TransformLoad /usr/local/bin

ENV AzureWebJobsScriptRoot=/home/site/wwwroot \
    AzureFunctionsJobHost__Logging__Console__IsEnabled=true