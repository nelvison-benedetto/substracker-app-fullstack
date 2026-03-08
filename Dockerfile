FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

COPY src/SubSnap.API/SubSnap.API.csproj src/SubSnap.API/
COPY src/SubSnap.Core/SubSnap.Core.csproj src/SubSnap.Core/
COPY src/SubSnap.Infrastructure/SubSnap.Infrastructure.csproj src/SubSnap.Infrastructure/
COPY src/SubSnap.Application/SubSnap.Application.csproj src/SubSnap.Application/

RUN dotnet restore src/SubSnap.API/SubSnap.API.csproj

COPY . .

RUN dotnet publish src/SubSnap.API/SubSnap.API.csproj \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0

WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080

EXPOSE 8080

ENTRYPOINT ["dotnet", "SubSnap.API.dll"]