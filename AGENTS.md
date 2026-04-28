# NormaQ Developer Notes

## Project Structure

A polyglot microservices repo with three main applications:

| Directory | Framework | Database | Notes |
|-----------|-----------|----------|-------|
| `src/dotnet-core/NormaQ/` | ASP.NET Core 10 | SQL Server | EF Core migrations |
| `src/php-app/` | Laravel 13 (PHP 8.3) | PostgreSQL | Vite + Tailwind frontend |
| `src/fastapi/` | FastAPI (Python) | (varies) | Minimal implementation |

## Running the Application

**Docker (complete stack):**
```bash
cd docker && docker-compose up --build
```

**Individual services:**
```bash
# .NET Core (requires SQL Server running)
cd src/dotnet-core/NormaQ && dotnet restore && dotnet watch run

# Laravel (requires PostgreSQL)
cd src/php-app && composer install && php artisan serve

# Laravel frontend (Vite + Tailwind)
cd src/php-app && npm run dev
```

## Database Migrations

```bash
# .NET Core (auto-runs on startup with retry logic)
dotnet ef migrations add <Name> && dotnet ef database update

# Laravel
php artisan migrate
php artisan make:migration <name>
```

## Environment Variables

- `.env` in `docker/` - Docker services config (SQL Server SA password, PostgreSQL credentials)
- `.env` in `src/php-app/` - Laravel config (DB: `pgsql`, port 5432)

## Testing

```bash
# Laravel
cd src/php-app && composer test

# .NET Core
cd src/dotnet-core/NormaQ && dotnet test
```

## Key Gotchas

- .NET migrations retry on startup (10 attempts, SQL Server takes time to start in Docker)
- Laravel `.env` DB_HOST must be `pgsql` when running in Docker
- nginx routes: `/` → .NET (C#), `/php-app` → Laravel (PHP)
- Solution file `Proyecto-DocsManager.sln` is the root orchestrator