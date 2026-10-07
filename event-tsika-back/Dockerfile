# Contexte de build : ce dossier (event-tsika-back/).
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY global.json .
COPY event-tsika-back/event-tsika-back.csproj event-tsika-back/
RUN dotnet restore event-tsika-back/event-tsika-back.csproj
COPY event-tsika-back/ event-tsika-back/
RUN dotnet publish event-tsika-back/event-tsika-back.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app .
# La base SQLite vit dans /data (volume) ; l'utilisateur non-root `app` doit pouvoir y écrire.
RUN mkdir /data && chown app /data
USER app
ENV ASPNETCORE_URLS=http://+:8080 \
    ConnectionStrings__Default="Data Source=/data/event-tsika.db"
VOLUME /data
EXPOSE 8080
ENTRYPOINT ["dotnet", "event-tsika-back.dll"]
