FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build-env
WORKDIR /app

COPY WebAPI_2/WebAPI_2.csproj ./WebAPI_2/
COPY ../WebAPI_2.DAL/WebAPI_2.DAL.csproj ./WebAPI_2.DAL/
RUN dotnet restore WebAPI_2/WebAPI_2.csproj

COPY WebAPI_2/. ./WebAPI_2/
COPY ../WebAPI_2.DAL/. ./WebAPI_2.DAL/
RUN dotnet publish WebAPI_2/WebAPI_2.csproj -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build-env /app/out .

EXPOSE 80

ENV ConnectionStrings__DefaultConnection="Server=host.docker.internal,1433;Database=Library;User=sa;Password=MyStr0ng!Pass123;TrustServerCertificate=True;"

ENTRYPOINT ["dotnet", "WebAPI_2.dll"]