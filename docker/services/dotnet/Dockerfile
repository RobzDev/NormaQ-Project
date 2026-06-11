FROM mcr.microsoft.com/dotnet/sdk:10.0 AS base
RUN apt-get update && apt-get install -y curl libreoffice-writer-nogui fonts-liberation fontconfig && rm -rf /var/lib/apt/lists/*
WORKDIR /app
EXPOSE 80

# ========== DESARROLLO ==========
FROM base AS dev
WORKDIR /src
COPY src/dotnet-core/NormaQ/NormaQ.csproj ./
RUN dotnet restore
WORKDIR /app
ENTRYPOINT ["dotnet", "watch", "run", "--non-interactive", "--", "--urls", "http://0.0.0.0:80"]

# ========== PRODUCCIÓN ==========
FROM base AS prod
WORKDIR /src
COPY . ./
RUN dotnet restore
RUN dotnet publish -c Release -o /app/publish
WORKDIR /app/publish
EXPOSE 8080
ENTRYPOINT ["dotnet", "NormaQ.dll"]