# Étape 1 : Configuration de l'image de base pour l'exécution
# Cette image contient uniquement le runtime .NET nécessaire pour exécuter l'application
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
# Exposition du port applicatif
EXPOSE 5002

# Étape 2 : Configuration de l'image de build
# Cette image contient le SDK .NET nécessaire pour compiler l'application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
# Copier les fichiers projet (contexte: racine du repo)
COPY ["AssociationService/AssociationService.csproj", "AssociationService/"]
COPY ["RessourcesPartagees/RessourcesPartagees.csproj", "RessourcesPartagees/"]
# Restaurer les dépendances
RUN dotnet restore "AssociationService/AssociationService.csproj"
# Copier tout le code source
COPY . .
# Positionner le dossier de build du service
WORKDIR "/src/AssociationService"
# Build Release
RUN dotnet build "AssociationService.csproj" -c Release -o /app/build

# Étape 3 : Publication de l'application
# Cette étape crée une version optimisée de l'application
FROM build AS publish
RUN dotnet publish "AssociationService.csproj" -c Release -o /app/publish

# Étape 4 : Création de l'image finale
# Cette image ne contient que les fichiers nécessaires pour exécuter l'application
FROM base AS final
WORKDIR /app
# Copie des fichiers publiés dans l'image finale
COPY --from=publish /app/publish .
# Commande de démarrage de l'application
ENTRYPOINT ["dotnet", "AssociationService.dll"] 