# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app

ENV ENABLE_ADMIN="false"
ENV ADMIN_USER="admin"
ENV ADMIN_PASS="admin"
ENV VW_URL=""
ENV VW_USERPW=""
ENV VW_CLIENTID=""
ENV VW_CLIENTSECRET=""
ENV VW_TIMEOUT="00:10:00"
ENV DB_SERVER=""
ENV DB_SERVERPORT="3306"
ENV DB_USER=""
ENV DB_PASSWORD=""
ENV DB_DATABASE="optShare"

EXPOSE 8080
# Overwrite NET ENV-Variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_HTTP_PORTS=8080
ENV DOTNET_RUNNING_IN_CONTAINER=true

# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["OTP_Share.csproj", "."]
RUN dotnet restore "./OTP_Share.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "./OTP_Share.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./OTP_Share.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish

# Install Bitwarden CLI
USER root
RUN apt-get update && apt-get install -y \
    curl \
    && curl -L "https://vault.bitwarden.com/download/?app=cli&platform=linux" -o /usr/local/bin/bw \
    && chmod +x /usr/local/bin/bw \
    && apt-get clean && rm -rf /var/lib/apt/lists/*

USER $APP_UID
ENTRYPOINT ["dotnet", "OTP_Share.dll"]
