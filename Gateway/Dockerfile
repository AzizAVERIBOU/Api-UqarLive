# Étape 1 : Configuration de l'image de base pour l'exécution
# Cette image contient uniquement le runtime .NET nécessaire pour exécuter l'application
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
# Exposition des ports pour HTTP (80) et HTTPS (443)
EXPOSE 80
EXPOSE 443

# Étape 2 : Configuration de l'image de build
# Cette image contient le SDK .NET nécessaire pour compiler l'application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
# Copie du fichier projet pour restaurer les dépendances
COPY ["Gateway/Gateway.csproj", "Gateway/"]
# Restauration des packages NuGet nécessaires
RUN dotnet restore "Gateway/Gateway.csproj"
# Copie de tous les fichiers source du projet
COPY . .
# Compilation de l'application en mode Release
RUN dotnet build "Gateway/Gateway.csproj" -c Release -o /app/build

# Étape 3 : Publication de l'application
# Cette étape crée une version optimisée de l'application
FROM build AS publish
RUN dotnet publish "Gateway/Gateway.csproj" -c Release -o /app/publish

# Étape 4 : Création de l'image finale
# Cette image ne contient que les fichiers nécessaires pour exécuter l'application
FROM base AS final
WORKDIR /app
# Copie des fichiers publiés dans l'image finale
COPY --from=publish /app/publish .
# Commande de démarrage de l'application
ENTRYPOINT ["dotnet", "Gateway.dll"] 