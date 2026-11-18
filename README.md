# SAV — Sales Administration System

SAV is a sales, inventory and invoicing system for small and medium businesses (SMBs) in Mexico.

It started in 2016 as my bachelor's thesis in Computer Systems: a desktop application built after surveying 177 SMBs in Puerto Vallarta, then tested in three real businesses. This repository is its **2026 rebuild**: the same business domain, redesigned with a modern stack and engineering practices.

> 🚧 Work in progress. The roadmap below shows what is done and what comes next.

## 2016 → 2026

| | Original (2016) | Rebuild (2026) |
|---|---|---|
| Language | VB.NET | C# on .NET 10 (LTS) |
| UI | Windows Forms (MDI desktop app) | React + TypeScript web app *(planned)* |
| Business logic | Inside the forms | Layered architecture: Domain → Application → Infrastructure → API |
| Data access | Hand-written SQL, SQL Server 2012 | EF Core 10 with migrations, SQL Server |
| Data model | Repeated columns (`PRECIO1..5`, `UTILIDAD1..5`) | Normalized tables; profit is computed, not stored |
| Errors | Message boxes | RFC 7807 problem responses |
| Distribution | `.exe` installer per machine | Docker + cloud deployment *(planned)* |
| Quality | Manual testing | Automated tests + CI on GitHub Actions *(planned)* |
| Invoicing | CFDI 3.2 via a PAC's SOAP API | CFDI 4.0 *(planned)* |

## Architecture

```
src/
  Sav.Domain           Entities and business rules (no dependencies)
  Sav.Application      Use cases, DTOs and persistence abstraction
  Sav.Infrastructure   EF Core DbContext, mappings and migrations
  Sav.Api              ASP.NET Core controllers, error handling, OpenAPI
```

Dependencies point inward: `Api → Application ← Infrastructure`, and everything depends on `Domain`.

## Roadmap

**v1: core operations**
- [x] Products: CRUD, up to 5 price levels, profit per level, soft delete
- [ ] Brands and product groups
- [ ] Customers, suppliers and branches
- [ ] Users, roles and JWT authentication
- [ ] Inventory movements (entries, exits, adjustments) and stock card per product
- [ ] Purchases, including import from CFDI XML files
- [ ] Point of sale: ticket and payment
- [ ] Quotes → orders
- [ ] Daily sales report

**Engineering**
- [x] Docker and Docker Compose (API + SQL Server + migrations)
- [ ] Unit and integration tests (xUnit)
- [ ] CI with GitHub Actions
- [ ] React + TypeScript front end
- [ ] Deployment to Azure

**v2 and beyond:** transfers between branches, serial numbers, PDF account statements, AI-assisted sales reports, CFDI 4.0 invoicing.

## Run it with Docker

Requirements: Docker Desktop (or Docker Engine with Compose v2).

```bash
cp .env.example .env   # then set your own SA password
docker compose up -d --build
```

This starts three services:

| Service | What it does |
|---|---|
| `sqlserver` | SQL Server 2025 (Developer), data kept in the `sqldata` volume, exposed on host port `14330` |
| `migrate` | Applies EF Core migrations with a [migration bundle](https://learn.microsoft.com/ef/core/managing-schemas/migrations/applying#bundles), then exits |
| `api` | Starts only after migrations succeed; listens on <http://localhost:8080> |

Open <http://localhost:8080/scalar> for the API docs and <http://localhost:8080/health> for the health check. Stop with `docker compose down` (add `-v` to also delete the database).

The image is a multi-stage build: compiled with the .NET SDK, run on the smaller ASP.NET runtime image as a non-root user.

## Run it locally (without Docker)

Requirements: .NET 10 SDK and a SQL Server instance. The easiest one is the container from the Docker setup (`docker compose up -d sqlserver`, port 14330).

The connection string is not in the repository. Store it with user secrets (kept outside the repo, under `%APPDATA%\Microsoft\UserSecrets`):

```bash
dotnet user-secrets set "ConnectionStrings:Sav" "Server=localhost,14330;Database=Sav;User Id=sa;Password=<your password>;TrustServerCertificate=True" --project src/Sav.Api
```

Then:

```bash
dotnet tool restore
dotnet tool run dotnet-ef database update --project src/Sav.Infrastructure --startup-project src/Sav.Api
dotnet run --project src/Sav.Api --launch-profile http
```

Then open <http://localhost:5192/scalar> for the interactive API docs, or use `src/Sav.Api/Sav.Api.http` from Visual Studio or VS Code.

Inside Docker the same setting comes from the `ConnectionStrings__Sav` environment variable (see `compose.yaml`).

## Notes

- Code and documentation are in English. The user interface will be in Spanish, since the target users are Mexican SMBs.
- © 2016–2026 Iván Hernández. All rights reserved. The source is public for portfolio and review purposes.
