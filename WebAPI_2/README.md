# WebAPI_2

This is the main ASP.NET Core Web API project for the Book Library system.

- **DAL**: Uses [WebAPI_2.DAL](https://github.com/Oleksandr-Zabo/WebAPI_2.DAL) for data access.
- **Database**: Expects a SQL Server instance (see connection string below).

## Functionality

This API provides endpoints for managing a digital book library, including:

- **Authentication & Authorization**
  - Register and login with JWT-based authentication
  - Admin creation and role-based access
- **Books**
  - List all books
  - Get book details by ID
  - (Likely) Add, update, and delete books (see BookController and BookService)
- **Authors**
  - List all authors
  - Get author details by ID
  - Create, update, and delete authors
- **Genres**
  - List all genres
  - Get genre details by ID
- **Users**
  - Register new users
  - (Likely) Manage user details (see UserController)

All endpoints are documented with Swagger/OpenAPI (auto-generated UI at `/swagger` when running).

## How to Run with Docker

1. Ensure you have Docker installed.
2. (Optional) Start a SQL Server container:
   ```sh
   docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=Your_password123" -p 1433:1433 -d mcr.microsoft.com/mssql/server:2022-latest
   ```
3. Build and run the API:
   ```sh
   docker build -t webapi2 .
   docker run -p 8080:80 --env ConnectionStrings__DefaultConnection="Server=host.docker.internal,1433;Database=Library;User=sa;Password=Your_password123;TrustServerCertificate=True;" webapi2
   ```
4. The API will be available at http://localhost:8080

## Configuration
- The connection string is set via environment variable `ConnectionStrings__DefaultConnection`.
- Update `appsettings.json` or use Docker `--env` as needed.

## Project Links
- [WebAPI_2 (this repo)](https://github.com/Oleksandr-Zabo/WebAPI_2)
- [WebAPI_2.DAL](https://github.com/Oleksandr-Zabo/WebAPI_2.DAL)
