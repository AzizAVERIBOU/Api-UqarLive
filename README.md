# 🚀 UQAR Live - API Backend Microservices

[![.NET 8.0](https://img.shields.io/badge/.NET-8.0-blue.svg)](https://dotnet.microsoft.com/download/dotnet/8.0)
[![Azure](https://img.shields.io/badge/Azure-App%20Service-blue.svg)](https://azure.microsoft.com/services/app-service/)
[![Docker](https://img.shields.io/badge/Docker-Containerized-blue.svg)](https://www.docker.com/)
[![Ocelot](https://img.shields.io/badge/Gateway-Ocelot-green.svg)](https://ocelot.readthedocs.io/)

> **Architecture microservices complète** pour l'application UQAR Live, développée en .NET 8.0 avec déploiement Azure et containerisation Docker.

## **Architecture**

### **Services Microservices**
- **🔐 AuthentificationService** (Port 5001) - Gestion des utilisateurs et authentification JWT
- **🏢 AssociationService** (Port 5002) - Gestion des associations étudiantes et événements
- **🛒 MarketService** (Port 5003) - Marketplace pour étudiants
- **📚 BibliothequeService** (Port 5004) - Réservation de salles d'étude
- **☕ CafetariaService** (Port 5005) - Gestion de la cafétéria et menus
- **🌐 Gateway** (Port 5000) - Point d'entrée unique avec routage Ocelot

### **Technologies et Stack Technique**

#### **Framework et Runtime**
- **.NET 8.0** - Framework principal avec support LTS
- **ASP.NET Core 8.0** - Framework web pour les APIs REST
- **C# 12** - Langage de programmation moderne
- **Nullable Reference Types** - Sécurité des types activée

#### **Base de Données et ORM**
- **SQL Server** - Base de données relationnelle principale
- **Entity Framework Core 9.0.6** - ORM moderne et performant
- **Entity Framework Tools** - Migrations et génération de code
- **Entity Framework Design** - Outils de conception

#### **API Gateway et Routage**
- **Ocelot 24.0.0** - API Gateway pour microservices
- **Microsoft.AspNetCore.OpenApi** - Support OpenAPI natif
- **Swashbuckle.AspNetCore** - Documentation Swagger/OpenAPI

#### **Authentification et Sécurité**
- **Microsoft.AspNetCore.Authentication.JwtBearer 8.0.2** - Authentification JWT
- **System.IdentityModel.Tokens.Jwt** - Gestion des tokens JWT
- **BCrypt** - Hachage sécurisé des mots de passe (via System.Security.Cryptography)

#### **Packages NuGet Détaillés**

##### **Entity Framework Core (Tous les Services)**
- **Microsoft.EntityFrameworkCore 9.0.6** - ORM principal pour l'accès aux données
- **Microsoft.EntityFrameworkCore.SqlServer 9.0.6** - Provider SQL Server pour EF Core
- **Microsoft.EntityFrameworkCore.Tools 9.0.6** - Outils CLI pour migrations et scaffolding
- **Microsoft.EntityFrameworkCore.Design 9.0.6** - Outils de conception pour EF Core

##### **API Gateway et Documentation**
- **Ocelot 24.0.0** (Gateway) - API Gateway pour routage microservices
- **Swashbuckle.AspNetCore 8.1.2** (Gateway, Market, Bibliotheque, Cafetaria) - Documentation Swagger/OpenAPI
- **Swashbuckle.AspNetCore 6.5.0** (Authentification) - Documentation Swagger version stable
- **Microsoft.AspNetCore.OpenApi 8.0.0** (Authentification, Association) - Support OpenAPI natif .NET 8
- **Microsoft.OpenApi.Readers 1.6.24** (Gateway) - Lecture et manipulation des spécifications OpenAPI

##### **Authentification et Sécurité**
- **Microsoft.AspNetCore.Authentication.JwtBearer 8.0.2** (Authentification) - Middleware d'authentification JWT
- **System.IdentityModel.Tokens.Jwt** (Authentification) - Gestion des tokens JWT
- **System.Security.Cryptography** (Authentification) - Hachage sécurisé des mots de passe

##### **Sérialisation et Communication**
- **Newtonsoft.Json 13.0.3** (Gateway, Market, Bibliotheque, Cafetaria) - Sérialisation JSON avancée
- **System.Text.Json** (Tous) - Sérialisation JSON native .NET
- **Microsoft.AspNetCore.Http** (Tous) - Gestion des requêtes HTTP

##### **Outils et Configuration**
- **Microsoft.Extensions.Configuration** (Tous) - Gestion de la configuration
- **Microsoft.Extensions.Configuration.Json** (Tous) - Configuration via fichiers JSON
- **Microsoft.Extensions.Configuration.EnvironmentVariables** (Tous) - Variables d'environnement
- **Microsoft.Extensions.Hosting** (Tous) - Gestion du cycle de vie des applications
- **Microsoft.Extensions.Logging** (Tous) - Framework de logging intégré
- **Microsoft.Extensions.DependencyInjection** (Tous) - Injection de dépendances native

##### **Packages par Service**

###### **AuthentificationService**
- **Microsoft.AspNetCore.Authentication.JwtBearer 8.0.2** - Middleware JWT
- **Microsoft.AspNetCore.OpenApi 8.0.0** - Support OpenAPI
- **Swashbuckle.AspNetCore 6.5.0** - Documentation Swagger
- **Newtonsoft.Json 13.0.3** - Sérialisation JSON

###### **Gateway**
- **Ocelot 24.0.0** - API Gateway principal
- **Microsoft.OpenApi.Readers 1.6.24** - Lecture OpenAPI
- **Swashbuckle.AspNetCore 8.1.2** - Documentation Swagger
- **Newtonsoft.Json 13.0.3** - Sérialisation JSON

###### **AssociationService**
- **Microsoft.AspNetCore.OpenApi 8.0.0** - Support OpenAPI
- **Swashbuckle.AspNetCore 6.4.0** - Documentation Swagger

###### **MarketService**
- **Swashbuckle.AspNetCore 8.1.2** - Documentation Swagger
- **Newtonsoft.Json 13.0.3** - Sérialisation JSON
- **Ocelot 24.0.0** - Support Gateway

###### **BibliothequeService**
- **Swashbuckle.AspNetCore 8.1.2** - Documentation Swagger
- **Newtonsoft.Json 13.0.3** - Sérialisation JSON
- **Ocelot 24.0.0** - Support Gateway

###### **CafetariaService**
- **Swashbuckle.AspNetCore 8.1.2** - Documentation Swagger
- **Newtonsoft.Json 13.0.3** - Sérialisation JSON
- **Ocelot 24.0.0** - Support Gateway

#### **Containerisation et Déploiement**
- **Docker** - Containerisation des services
- **Microsoft.NET.Sdk.Web** - SDK pour applications web ASP.NET Core
- **Azure App Service** - Plateforme de déploiement cloud
- **Azure Container Registry** - Registry pour les images Docker

#### **Outils de Développement**
- **Visual Studio 2022** - IDE principal
- **Visual Studio Code** - Éditeur alternatif
- **Git** - Contrôle de version
- **Azure CLI** - Gestion des ressources Azure
- **Docker Desktop** - Environnement de développement local

#### **Monitoring et Logging**
- **Microsoft.Extensions.Logging** - Framework de logging intégré
- **Microsoft.Extensions.Logging.Console** - Provider de logging console
- **Microsoft.Extensions.Logging.Debug** - Provider de logging debug
- **Azure App Service Logs** - Logs intégrés Azure

#### **Communication Inter-Services**
- **HttpClientFactory** - Gestion des clients HTTP
- **System.Text.Json** - Sérialisation JSON native .NET
- **Microsoft.AspNetCore.Http** - Gestion des requêtes HTTP

#### **Documentation et API**
- **OpenAPI 3.0** - Spécification des APIs
- **Swagger UI** - Interface utilisateur pour la documentation
- **XML Documentation** - Documentation du code C#

#### **Configuration et Environnement**
- **Microsoft.Extensions.Configuration** - Gestion de la configuration
- **Microsoft.Extensions.Configuration.Json** - Configuration via fichiers JSON
- **Microsoft.Extensions.Configuration.EnvironmentVariables** - Variables d'environnement
- **Microsoft.Extensions.Hosting** - Gestion du cycle de vie des applications

#### **Sécurité et Validation**
- **Data Annotations** - Validation des modèles
- **ModelState** - Validation des données de requête
- **CORS (Cross-Origin Resource Sharing)** - Gestion des requêtes cross-origin
- **HTTPS** - Communication chiffrée

#### **Performance et Optimisation**
- **Entity Framework Core Query Optimization** - Optimisation des requêtes
- **Async/Await Pattern** - Programmation asynchrone
- **Dependency Injection** - Injection de dépendances native
- **Memory Management** - Gestion automatique de la mémoire

#### **Architecture et Patterns**
- **Microservices Architecture** - Architecture distribuée
- **API Gateway Pattern** - Point d'entrée unique
- **Repository Pattern** - Abstraction de l'accès aux données
- **Dependency Injection Pattern** - Inversion de contrôle
- **Configuration Pattern** - Gestion centralisée de la configuration


#### **Configuration des Services**

##### **Variables d'Environnement Requises**
```bash
# Base de données (tous les services)
ConnectionStrings__DefaultConnection="Server=...;Database=...;Trusted_Connection=true;"

# JWT (AuthentificationService)
JwtSettings__SecretKey="votre-clé-secrète-jwt"
JwtSettings__Issuer="UQAR-Live"
JwtSettings__Audience="UQAR-Live-Users"

# Ports (optionnel)
PORT=5000
WEBSITES_PORT=5000

# Azure (production)
ASPNETCORE_ENVIRONMENT=Production
```

##### **Configuration Docker**
```dockerfile
# Image de base
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# Ports exposés
EXPOSE 80
EXPOSE 443
EXPOSE 5000

# Variables d'environnement
ENV ASPNETCORE_URLS=http://+:5000
ENV ASPNETCORE_ENVIRONMENT=Production
```

#### **Outils de Développement Recommandés**

##### **Extensions Visual Studio**
- **Entity Framework Core Power Tools** - Visualisation des modèles
- **Swagger Generator** - Génération automatique de documentation
- **Azure App Service Tools** - Déploiement Azure

##### **Extensions VS Code**
- **C# Dev Kit** - Support C# complet
- **REST Client** - Test des APIs
- **Docker** - Gestion des conteneurs
- **Azure Tools** - Gestion des ressources Azure

##### **Outils CLI**
```bash
# Entity Framework
dotnet ef migrations add NomMigration
dotnet ef database update

# Swagger
dotnet swagger tofile --output swagger.json

# Docker
docker build -t service-name .
docker run -p 5000:5000 service-name

# Azure
az webapp restart --name service-name --resource-group resource-group
```

## **Déploiement en Production**

### **URLs des Services**
- **Gateway Principal** : https://uqarlivegateway-hbayescme8ckf7e8.canadacentral-01.azurewebsites.net
- **Documentation API** : https://uqarlivegateway-hbayescme8ckf7e8.canadacentral-01.azurewebsites.net/swagger

### **Services Individuels**
- **Authentification** : https://uqarliveauth-czhfbpapdfcxhuht.canadacentral-01.azurewebsites.net/swagger
- **Associations** : https://uqarliveassociation-hhfucadyb0dtaugm.canadacentral-01.azurewebsites.net/swagger
- **Market** : https://uqarlivemarket-cpapfjgzbhdtesf8.canadacentral-01.azurewebsites.net/swagger
- **Bibliothèque** : https://uqarlivebibliotheque-emg2a5dtbchha9hr.canadacentral-01.azurewebsites.net/swagger
- **Cafétéria** : https://uqarlivecafetaria-gqbyd6h4f2faesfv.canadacentral-01.azurewebsites.net/swagger

## **Installation et Développement**

### **Prérequis**
- .NET 8.0 SDK ou supérieur
- Visual Studio 2022 ou Visual Studio Code
- Docker Desktop (optionnel)
- SQL Server (local ou Azure)

### **Installation Locale**

1. **Cloner le repository**
```bash
git clone https://github.com/votre-username/API-Backend.git
cd API-Backend
```

2. **Restaurer les dépendances**
```bash
dotnet restore
```

3. **Configurer les bases de données**
```bash
# Pour chaque service, exécuter les migrations
dotnet ef database update --project AuthentificationService
dotnet ef database update --project AssociationService
dotnet ef database update --project MarketService
dotnet ef database update --project BibliothequeService
dotnet ef database update --project CafetariaService
```

4. **Lancer les services**
```bash
# Terminal 1 - Gateway
dotnet run --project Gateway

# Terminal 2 - Authentification
dotnet run --project AuthentificationService

# Terminal 3 - Association
dotnet run --project AssociationService

# Terminal 4 - Market
dotnet run --project MarketService

# Terminal 5 - Bibliothèque
dotnet run --project BibliothequeService

# Terminal 6 - Cafétéria
dotnet run --project CafetariaService
```

### **Avec Docker**

```bash
# Construire toutes les images
docker-compose build

# Lancer tous les services
docker-compose up -d
```

## **Documentation API**

### **Accès à la Documentation**
- **Swagger Centralisé** : https://uqarlivegateway-hbayescme8ckf7e8.canadacentral-01.azurewebsites.net/swagger
- **Swagger Individuel** : Chaque service a sa propre documentation Swagger

### **Endpoints Principaux**

#### **Authentification**
- `POST /auth/Inscription` - Inscription d'un nouvel utilisateur
- `POST /auth/Connexion` - Connexion utilisateur
- `GET /auth/VerifierUtilisateur/{codePermanent}` - Vérification utilisateur

#### **Associations**
- `GET /association/ObtenirAssociations` - Liste des associations
- `POST /association/CreerAssociation` - Créer une association
- `GET /association/ObtenirEvenements/{id}` - Événements d'une association

#### **Market**
- `GET /market/produits` - Liste des produits
- `POST /market/produits` - Créer un produit
- `GET /market/categories` - Catégories disponibles

#### **Bibliothèque**
- `GET /bibliotheque/salles` - Liste des salles
- `GET /bibliotheque/salles/disponibles` - Salles disponibles
- `POST /bibliotheque/reservations` - Réserver une salle

#### **Cafétéria**
- `GET /cafetaria/menuitems` - Plats disponibles
- `GET /cafetaria/menu/aujourdhui` - Menu du jour
- `GET /cafetaria/horaires` - Horaires d'ouverture

## 🔧 **Configuration**

### **Variables d'Environnement**
```bash
# Base de données
ConnectionStrings__DefaultConnection="Server=...;Database=...;Trusted_Connection=true;"

# JWT
JwtSettings__SecretKey="votre-clé-secrète-jwt"

# Ports (optionnel)
PORT=5000
```

### **Configuration Azure**
- **Région** : Canada Central
- **Plan** : App Service Plan
- **Runtime** : .NET 8.0
- **HTTPS** : Activé automatiquement

## **Structure du Projet**

```
API-Backend/
├── Gateway/                    # API Gateway (Ocelot)
│   ├── Controllers/           # Contrôleurs proxy
│   ├── Services/              # Services d'agrégation
│   ├── ocelot.json           # Configuration de routage
│   └── Program.cs            # Point d'entrée
├── AuthentificationService/    # Service d'authentification
│   ├── Controllers/           # AuthentificationController
│   ├── Models/               # Utilisateur, JWT, etc.
│   ├── Data/                 # DbContext
│   └── Services/             # Services métier
├── AssociationService/         # Service des associations
├── MarketService/             # Service marketplace
├── BibliothequeService/       # Service bibliothèque
├── CafetariaService/          # Service cafétéria
├── RessourcesPartagees/       # Modèles partagés
└── API-Backend.sln           # Solution principale
```

## **Tests**

### **Tests des Services**
```bash
# Tester la connectivité
curl https://uqarlivegateway-hbayescme8ckf7e8.canadacentral-01.azurewebsites.net/health

# Tester l'authentification
curl -X POST https://uqarlivegateway-hbayescme8ckf7e8.canadacentral-01.azurewebsites.net/auth/test
```

### **Tests Locaux**
```bash
# Lancer les tests unitaires (si disponibles)
dotnet test
```

## **Monitoring et Logs**

### **Health Checks**
- **Gateway** : `/health`
- **Services** : Chaque service expose un endpoint de santé

### **Logs**
- **Azure App Service** : Logs intégrés
- **Application Insights** : Monitoring avancé (optionnel)
- **Console Logging** : Logs structurés avec ILogger

## **Contribution**

### **Standards de Code**
- **C#** : Suivre les conventions Microsoft
- **API** : RESTful avec OpenAPI/Swagger
- **Tests** : Tests unitaires pour les nouvelles fonctionnalités
- **Documentation** : Mettre à jour la documentation API

## **Licence**

Ce projet est sous licence MIT. 

- **Version anglaise** : [LICENSE](LICENSE)
- **Version française** : [LICENSE-FR.md](LICENSE-FR.md)

### **Droits accordés**
- Modification et distribution
- Utilisation privée
- Sous-licenciement

### **Conditions**
- Inclure le copyright et la licence
- Aucune garantie fournie

## 👥 **Équipe**

- **Développement** : Aziz AVERIBOU 
- **Architecture** : Microservices .NET 8.0
- **Déploiement** : Azure App Service

## **Support**

- **Issues** : [GitHub Issues](https://github.com/AzizAVERIBOU/Api-UqarLive.git)
- **Documentation** : [Wiki du projet](https://github.com/AzizAVERIBOU/Api-UqarLive.git)
- **Email** : azizaveribou6123@gmail.com 

---

**🚀 UQAR Live - Architecture Microservices Moderne et Scalable** 

