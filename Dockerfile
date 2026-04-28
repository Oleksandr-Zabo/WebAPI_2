FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build-env
WORKDIR /app

# Копіюємо все для збірки
COPY . .

# Restore, build, publish
RUN dotnet restore WebAPI_2/WebAPI_2.csproj
RUN dotnet publish WebAPI_2/WebAPI_2.csproj -c Release -o out

# Фінальний образ
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build-env /app/out .

EXPOSE 8080
ENV ASPNETCORE_ENVIRONMENT=Development
ENV ConnectionStrings__DefaultConnection="Server=db,1433;Database=Library;User=sa;Password=MyStr0ng!Pass123;TrustServerCertificate=True;"
ENTRYPOINT ["dotnet", "WebAPI_2.dll"]
