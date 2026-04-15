# UQAR Live — API Backend (microservices)

[![.NET 8](https://img.shields.io/badge/.NET-8.0-512BD4)](https://dotnet.microsoft.com/download/dotnet/8.0)
[![Azure App Service](https://img.shields.io/badge/Azure-App%20Service-0078D4)](https://azure.microsoft.com/services/app-service/)
[![Ocelot](https://img.shields.io/badge/Gateway-Ocelot-009639)](https://github.com/ThreeMammals/Ocelot)

Backend **microservices** pour l’application **UQAR Live** : APIs REST en **ASP.NET Core** (.NET 8), routage centralisé via **Ocelot**, persistance **SQL Server** avec **Entity Framework Core**, déploiement cible **Azure App Service**.

**Dépôt :** [https://github.com/AzizAVERIBOU/Api-UqarLive](https://github.com/AzizAVERIBOU/Api-UqarLive)

**Liens personnels :** [Portfolio (Vercel)](https://profil-aziz.vercel.app/) · [LinkedIn](https://www.linkedin.com/in/aziz-averibou-51b782323/)

---

## Sommaire

- [Architecture](#architecture)
- [Stack technique](#stack-technique)
- [Structure du dépôt](#structure-du-dépôt)
- [Prérequis et installation](#prérequis-et-installation)
- [Exécution locale](#exécution-locale)
- [Configuration](#configuration)
- [Docker](#docker)
- [Environnements Azure (référence)](#environnements-azure-référence)
- [Documentation des API](#documentation-des-api)
- [Contribution et licence](#contribution-et-licence)
- [Auteur](#auteur)

---

## Architecture

| Composant | Rôle | Port local (exemple) |
|-----------|------|-------------------------|
| **Gateway** | Point d’entrée unique, **Ocelot** (`ocelot.json`), Swagger UI, agrégation Swagger | `5000` (ou `PORT` / `WEBSITES_PORT`) |
| **AuthentificationService** | Comptes, JWT (`JwtBearer`) | `5001` |
| **AssociationService** | Associations, adhésions, événements | `5002` |
| **MarketService** | Marketplace | `5003` |
| **BibliothequeService** | Bibliothèque / salles | `5004` |
| **CafetariaService** | Cafétéria, menus | `5005` |
| **RessourcesPartagees** | Bibliothèque de classes partagée (référencée par certains services) | — |

Chaque service métier expose ses propres contrôleurs et, en général, un **DbContext** EF Core dédié (bases distinctes côté configuration).

---

## Stack technique

- **Runtime / framework :** .NET **8.0**, C# avec nullable reference types.
- **ORM :** **Entity Framework Core 9.0.11** + provider **Microsoft.EntityFrameworkCore.SqlServer**.
- **Gateway :** **Ocelot 24.1.0**, **Swashbuckle.AspNetCore 10.1.7** (OpenAPI via **Microsoft.OpenApi 2.x**).
- **APIs :** **Swashbuckle.AspNetCore 10.1.7** sur les services web ; **Microsoft.AspNetCore.OpenApi 8.0.22** là où il est référencé.
- **Auth :** **Microsoft.AspNetCore.Authentication.JwtBearer 8.0.22**.
- **JSON :** **Newtonsoft.Json 13.0.4** (là où le package est présent) et **System.Text.Json** (ASP.NET Core).

> Les versions **EF Core 10** et **ASP.NET Core 10** sur NuGet ciblent **.NET 10** ; ce dépôt reste sur **.NET 8** avec les dernières versions **compatibles net8.0**.

---

## Structure du dépôt

```
API-Backend/
├── API-Backend.sln
├── Gateway/                 # Ocelot, Swagger, agrégation OpenAPI
├── AuthentificationService/
├── AssociationService/
├── MarketService/
├── BibliothequeService/
├── CafetariaService/
├── RessourcesPartagees/     # Types partagés (EF référencé si besoin)
├── .github/workflows/       # CI/CD Azure (selon branches)
└── LICENSE
```

---

## Prérequis et installation

1. **SDK :** [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) ou supérieur (pour compiler un projet `net8.0`).
2. **SQL Server** (local, conteneur ou Azure SQL) et chaînes de connexion configurées.
3. **Git**

```bash
git clone https://github.com/AzizAVERIBOU/Api-UqarLive.git
cd Api-UqarLive
dotnet restore API-Backend.sln
dotnet build API-Backend.sln -c Release
```

En cas de contrainte mémoire sur la machine de build, vous pouvez limiter le parallélisme MSBuild : `dotnet build API-Backend.sln -c Release -m:1`.

---

## Exécution locale

Exemple pour lancer un service depuis la racine du dépôt :

```bash
dotnet run --project Gateway
dotnet run --project AuthentificationService
```

Les ports réels dépendent de `launchSettings.json`, de `UseUrls` dans `Program.cs` (certains services fixent un port) et des variables d’environnement (**`PORT`** / **`WEBSITES_PORT`** sur le Gateway, aligné avec Azure App Service).

**Santé (exemple) :** le service d’authentification expose des routes simples de statut (`/` et `/health` dans `Program.cs`). Pour le Gateway, privilégier **Swagger** ou la route **Home** selon la configuration des contrôleurs.

---

## Configuration

### Chaînes de connexion

Utiliser `ConnectionStrings:DefaultConnection` (fichiers `appsettings.json` / `appsettings.Development.json` ou variables d’environnement `ConnectionStrings__DefaultConnection`).

### JWT (AuthentificationService)

Configurer la section **JwtSettings** (notamment la clé secrète), par exemple via variables d’environnement ou secrets utilisateur, **sans** commiter de secrets.

### Gateway (Ocelot)

Fichiers **`Gateway/ocelot.json`** (et variantes Azure si présentes) : chemins amont / aval vers les instances des microservices.

---

## Docker

Des **Dockerfile** sont présents par service (Gateway et services métier). Exemple de build local :

```bash
docker build -t uqarlive-gateway -f Gateway/Dockerfile Gateway
```

Il n’y a pas de fichier `docker-compose` à la racine du dépôt ; l’orchestration locale multi-conteneurs reste à votre convenance.

---

## Environnements Azure (référence)

Les URL ci-dessous correspondent à une configuration **App Service (Canada Central)** documentée dans le projet ; elles peuvent évoluer selon votre abonnement Azure.

| Rôle | URL (exemple) |
|------|----------------|
| Gateway | `https://uqarlivegateway-hbayescme8ckf7e8.canadacentral-01.azurewebsites.net` |
| Swagger Gateway | …`/swagger` |
| Authentification | `https://uqarliveauth-czhfbpapdfcxhuht.canadacentral-01.azurewebsites.net` |
| Associations | `https://uqarliveassociation-hhfucadyb0dtaugm.canadacentral-01.azurewebsites.net` |
| Market | `https://uqarlivemarket-cpapfjgzbhdtesf8.canadacentral-01.azurewebsites.net` |
| Bibliothèque | `https://uqarlivebibliotheque-emg2a5dtbchha9hr.canadacentral-01.azurewebsites.net` |
| Cafétéria | `https://uqarlivecafetaria-gqbyd6h4f2faesfv.canadacentral-01.azurewebsites.net` |

Les routes exposées publiquement passent en pratique par le **Gateway** (préfixes du type `/auth/…`, `/association/…`, etc., selon `ocelot.json`).

---

## Documentation des API

- **Swagger UI** est activé sur les services qui configurent Swashbuckle.
- Le Gateway peut exposer une documentation agrégée (selon implémentation : contrôleur Swagger / agrégateur OpenAPI).

Pour le détail des chemins et des corps de requête, se référer au **Swagger** du service concerné ou à la doc OpenAPI générée.

---

## Migrations Entity Framework

Si des migrations sont utilisées pour un service :

```bash
dotnet ef database update --project AuthentificationService
# Adapter le nom du projet (.csproj) selon le service
```

La présence du dossier `Migrations/` peut être ignorée par Git selon `.gitignore` ; en cas d’absence de migrations versionnées, suivre la convention de votre équipe.

---

## Contribution et licence

- **Issues :** [github.com/AzizAVERIBOU/Api-UqarLive/issues](https://github.com/AzizAVERIBOU/Api-UqarLive/issues)
- **Licence :** voir le fichier [LICENSE](LICENSE) à la racine du dépôt.

## Auteur

**Aziz AVERIBOU**

- **Portfolio :** [https://profil-aziz.vercel.app/](https://profil-aziz.vercel.app/)
- **LinkedIn :** [https://www.linkedin.com/in/aziz-averibou-51b782323/](https://www.linkedin.com/in/aziz-averibou-51b782323/)
- **Courriel :** azizaveribou6123@gmail.com

---

*UQAR Live — APIs microservices .NET 8, gateway Ocelot, persistance SQL Server.*
