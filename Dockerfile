# Dockerfile for WebAPI_2 and WebAPI_2.DAL
# Use official .NET SDK image for build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY WebAPI_2/WebAPI_2.csproj ./WebAPI_2/
COPY WebAPI_2.DAL/WebAPI_2.DAL.csproj ./WebAPI_2.DAL/
RUN dotnet restore WebAPI_2/WebAPI_2.csproj

# Copy everything else and build
COPY WebAPI_2/. ./WebAPI_2/
COPY WebAPI_2.DAL/. ./WebAPI_2.DAL/
RUN dotnet publish WebAPI_2/WebAPI_2.csproj -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build-env /app/out .

# Expose port (change if your app uses a different port)
EXPOSE 80

# Set environment variables for SQL Server (update as needed)
ENV ConnectionStrings__DefaultConnection="Server=host.docker.internal,1433;Database=Library;User=sa;Password=MyStr0ng!Pass123;TrustServerCertificate=True;"

# Start the API
ENTRYPOINT ["dotnet", "WebAPI_2.dll"]
