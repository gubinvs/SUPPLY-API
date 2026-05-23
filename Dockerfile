FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080

# SDK build stage
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG configuration=Release
WORKDIR /src

COPY ["SUPPLY-API.csproj", "./"]
RUN dotnet restore "SUPPLY-API.csproj"

COPY . .
WORKDIR "/src/."
RUN dotnet build "SUPPLY-API.csproj" -c $configuration -o /app/build

# publish stage
FROM build AS publish
ARG configuration=Release
RUN dotnet publish "SUPPLY-API.csproj" -c $configuration -o /app/publish /p:UseAppHost=false

# final stage
FROM base AS final
WORKDIR /app

COPY --from=publish /app/publish .

# создаём папку для файлов и даём права
USER root
RUN mkdir -p /app/Files && chown -R app:app /app/Files

# возвращаем пользователя
USER app

ENTRYPOINT ["dotnet", "SUPPLY-API.dll"]