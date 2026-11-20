# Etapa 1: compilar
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# 1) Solo los archivos de proyecto, para cachear el restore
COPY dotnet-tools.json ./
COPY src/Sav.Domain/Sav.Domain.csproj src/Sav.Domain/
COPY src/Sav.Application/Sav.Application.csproj src/Sav.Application/
COPY src/Sav.Infrastructure/Sav.Infrastructure.csproj src/Sav.Infrastructure/
COPY src/Sav.Api/Sav.Api.csproj src/Sav.Api/
RUN dotnet restore src/Sav.Api/Sav.Api.csproj && dotnet tool restore

# 2) Ahora sí, el código
COPY src/ src/
RUN dotnet publish src/Sav.Api/Sav.Api.csproj -c Release -o /app --no-restore

# 3a) Genera el bundle, reusando todo lo que ya compiló "build"
FROM build AS migrations-build
RUN ConnectionStrings__Sav="Server=design-time;Database=Sav" \
      dotnet tool run dotnet-ef migrations bundle \
      --project src/Sav.Infrastructure \
      --startup-project src/Sav.Api \
      --configuration Release \
      --output /app/efbundle

# 3b) Imagen chica que solo trae el ejecutable
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS migrations
WORKDIR /app
COPY --from=migrations-build /app/efbundle .
USER $APP_UID
ENTRYPOINT ["./efbundle"]





# Etapa 2: ejecutar
FROM mcr.microsoft.com/dotnet/aspnet:10.0  AS runtime
WORKDIR /app
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*
COPY --from=build /app .
USER $APP_UID
EXPOSE 8080
ENTRYPOINT ["dotnet", "Sav.Api.dll"]

