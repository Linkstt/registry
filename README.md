# Registry Service

Software Registry for the AllServices platform — products, versions, manifests, chunked-download metadata.

## Architecture

```
Registry.Domain        → Entities, Enums, Repository interfaces
Registry.Application   → Services, DTOs, Validators, IObjectStorage / ICurrentUser
Registry.Infrastructure→ EF Core repositories, MinIO storage, PostgreSQL DbContext
Registry.Api           → ASP.NET Core 9 host, Controllers, Auth policies, Middleware
```

## Endpoints

| Method | Route | Policy | Description |
|--------|-------|--------|-------------|
| GET | `/api/products` | ProductRead | List products (paginated, filtered) |
| POST | `/api/products` | ProductWrite | Create product |
| GET | `/api/products/{id}` | ProductRead | Get product by ID |
| GET | `/api/products/by-slug/{slug}` | ProductRead | Get product by slug |
| PATCH | `/api/products/{id}` | ProductWrite | Update product |
| DELETE | `/api/products/{id}` | ProductWrite | Soft-delete (delist) |
| POST | `/api/products/{id}/suspend` | AdminOnly | Suspend product |
| POST | `/api/products/{id}/unsuspend` | AdminOnly | Unsuspend product |
| GET | `/api/products/{id}/versions` | VersionRead | List versions |
| POST | `/api/products/{id}/versions` | VersionWrite | Create version |
| GET | `/api/products/{id}/versions/{vid}` | VersionRead | Get version |
| POST | `/api/products/{id}/versions/{vid}/yank` | VersionWrite | Yank version |
| POST | `/api/products/{id}/versions/{vid}/status` | AdminOnly | Transition status |
| GET | `/api/manifests/{vid}/{platform}/{arch}` | ManifestRead | Get binary manifest with signed chunk URLs |
| GET | `/api/categories` | CategoryRead | Category tree |
| GET | `/api/categories/{slug}` | CategoryRead | Category by slug |
| POST | `/api/assets/upload` | AssetWrite | Initiate presigned upload |
| GET | `/api/assets/product/{id}` | ProductRead | List product assets |
| DELETE | `/api/assets/{id}` | AssetWrite | Delete asset |

## Health Checks

- `/health/live` — Liveness
- `/health/ready` — Readiness (includes PostgreSQL)
- `/metrics` — Prometheus

## Environment Variables (Coolify)

```
DATABASE_URL=Host=...;Port=5432;Database=allservices_registry;Username=registry_user;Password=...
MINIO_ENDPOINT=s3.allservices.cc
MINIO_ACCESS_KEY=...
MINIO_SECRET_KEY=...
CDN_BASE_URL=https://cdn.allservices.cc
AUTH_AUTHORITY=https://auth.allservices.cc
ASPNETCORE_ENVIRONMENT=Production
```

## Run locally

```bash
docker compose up -d
```

## Build

```bash
dotnet build
```
