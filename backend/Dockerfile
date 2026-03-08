# -------- BUILD STAGE --------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build  #usa image docker ufficiale 8.0

WORKDIR /src

# copia solo i file progetto per sfruttare il caching docker
COPY src/SubSnap.API/SubSnap.API.csproj src/SubSnap.API/
COPY src/SubSnap.Core/SubSnap.Core.csproj src/SubSnap.Core/
COPY src/SubSnap.Infrastructure/SubSnap.Infrastructure.csproj src/SubSnap.Infrastructure/
COPY src/SubSnap.Application/SubSnap.Application.csproj src/SubSnap.Application/

RUN dotnet restore src/SubSnap.API/SubSnap.API.csproj

# copia il resto del codice
COPY . .

RUN dotnet publish src/SubSnap.API/SubSnap.API.csproj \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false
    #dotnet publish fa compila il prj->crea i files eseguibili -> copia le dipendenze.

# -------- RUNTIME STAGE --------
FROM mcr.microsoft.com/dotnet/aspnet:8.0

WORKDIR /app

COPY --from=build /app/publish .
   #/app/publish viene copiato in /app

# ASP.NET ascolta su tutte le interfacce
ENV ASPNETCORE_URLS=http://+:8080
  #dice a asp.net core di ascoltare su tutte le interfaccie sulla porta 8080. '+' significa 0.0.0.0 cioe accessibilità da fuori!quindi non devi settre esplicitamente nel code program.cs builder.WebHost.UseUrls("http://0.0.0.0:8080");
EXPOSE 8080
  #dice a docker che questa app usa la porta 8080!
ENTRYPOINT ["dotnet", "SubSnap.API.dll"]
  #questo è il comando che parte quando il container si avvia.