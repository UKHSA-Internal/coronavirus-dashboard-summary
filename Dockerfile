﻿FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
LABEL maintainer="Pouria Hadjibagheri <Pouria.Hadjibagheri@phe.gov.uk>"

WORKDIR /app

EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
LABEL maintainer="Pouria Hadjibagheri <Pouria.Hadjibagheri@phe.gov.uk>"

WORKDIR /src
COPY ["coronavirus_dashboard_summary/coronavirus_dashboard_summary.fsproj", "coronavirus_dashboard_summary/"]

RUN dotnet restore "coronavirus_dashboard_summary/coronavirus_dashboard_summary.fsproj"
COPY . .
WORKDIR "/src/coronavirus_dashboard_summary"

RUN dotnet build "coronavirus_dashboard_summary.fsproj" -c Release -o /app/build

FROM build AS publish
LABEL maintainer="Pouria Hadjibagheri <Pouria.Hadjibagheri@phe.gov.uk>"

RUN dotnet publish "coronavirus_dashboard_summary.fsproj" -c Release -o /app/publish

FROM base AS final
LABEL maintainer="Pouria Hadjibagheri <Pouria.Hadjibagheri@phe.gov.uk>"

ENV Environment = "Production"

WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "coronavirus_dashboard_summary.dll"]
