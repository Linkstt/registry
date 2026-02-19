FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build
WORKDIR /src

# Copy solution and project files
COPY Registry.AllServices.sln ./
COPY src/Registry.Domain/Registry.Domain.csproj src/Registry.Domain/
COPY src/Registry.Application/Registry.Application.csproj src/Registry.Application/
COPY src/Registry.Infrastructure/Registry.Infrastructure.csproj src/Registry.Infrastructure/
COPY src/Registry.Api/Registry.Api.csproj src/Registry.Api/
RUN dotnet restore

# Copy source and build
COPY . .
RUN dotnet publish src/Registry.Api/Registry.Api.csproj -c Release -o /app --no-restore

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS runtime
WORKDIR /app

RUN addgroup -S appgroup && adduser -S appuser -G appgroup

COPY --from=build /app .

RUN chown -R appuser:appgroup /app
USER appuser

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080

HEALTHCHECK --interval=30s --timeout=5s --start-period=10s --retries=3 \
    CMD wget --no-verbose --tries=1 --spider http://localhost:8080/health/live || exit 1

ENTRYPOINT ["dotnet", "Registry.Api.dll"]
