FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["src/HealthCare.API/HealthCare.API.csproj", "src/HealthCare.API/"]
COPY ["src/HealthCare.Application/HealthCare.Application.csproj", "src/HealthCare.Application/"]
COPY ["src/HealthCare.Domain/HealthCare.Domain.csproj", "src/HealthCare.Domain/"]
COPY ["src/HealthCare.Infrastructure/HealthCare.Infrastructure.csproj", "src/HealthCare.Infrastructure/"]

RUN dotnet restore "src/HealthCare.API/HealthCare.API.csproj"

COPY . .
WORKDIR "/src/src/HealthCare.API"
RUN dotnet publish "HealthCare.API.csproj" -c Release -o /app/publish --no-restore

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "HealthCare.API.dll"]
