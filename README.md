# UQAR Live — API Backend (microservices)

[![.NET 8](https://img.shields.io/badge/.NET-8.0-512BD4)](https://dotnet.microsoft.com/download/dotnet/8.0)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-18-4169E1)](https://www.postgresql.org/)
[![Ocelot](https://img.shields.io/badge/Gateway-Ocelot-009639)](https://github.com/ThreeMammals/Ocelot)
[![CI](https://github.com/AzizAVERIBOU/Api-UqarLive/actions/workflows/ci.yml/badge.svg)](https://github.com/AzizAVERIBOU/Api-UqarLive/actions/workflows/ci.yml)

Backend **microservices** pour l’application **UQAR Live** : APIs REST en **ASP.NET Core** (.NET 8), routage centralisé via **Ocelot**, persistance **PostgreSQL** avec **Entity Framework Core** et **Npgsql**.

Le dépôt compile la solution et exécute les tests d’authentification avec **GitHub Actions**. Il n’y a pas de déploiement automatique.

Le service d’authentification est en ligne : [https://api-uqarlive.onrender.com](https://api-uqarlive.onrender.com). Les autres services ne sont pas encore publiés.

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
- [Intégration continue](#intégration-continue)
- [Documentation des API](#documentation-des-api)
- [Migrations Entity Framework](#migrations-entity-framework)
- [Contribution et licence](#contribution-et-licence)
- [Auteur](#auteur)

---

## Architecture

| Composant | Rôle | Port local (exemple) |
|-----------|------|-------------------------|
| **Gateway** | Point d’entrée unique, **Ocelot** (`ocelot.json`), Swagger UI | `5000` (ou `PORT`) |
| **AuthentificationService** | Comptes, JWT (`JwtBearer`). Public : [api-uqarlive.onrender.com](https://api-uqarlive.onrender.com) | `5001` |
| **AssociationService** | Associations, adhésions, événements | `5002` |
| **MarketService** | Marketplace | `5003` |
| **BibliothequeService** | Bibliothèque / salles | `5004` |
| **CafetariaService** | Cafétéria, menus et horaires | `5005` |
| **RessourcesPartagees** | Bibliothèque de classes partagée | — |

Chaque service métier a son propre **DbContext**. Tous lisent `ConnectionStrings:DefaultConnection` et peuvent partager une même base PostgreSQL.

La passerelle envoie `/auth/…` vers `https://api-uqarlive.onrender.com/api/Authentification/…`. Association, Marché et Bibliothèque appellent cette même adresse pour `VerifierUtilisateur`.

---

## Stack technique

- **Runtime / framework :** .NET **8.0**, C# avec nullable reference types.
- **ORM :** **Entity Framework Core 9.0.11** + provider **Npgsql.EntityFrameworkCore.PostgreSQL 9.0.4**.
- **Base :** **PostgreSQL 18**.
- **Gateway :** **Ocelot 24.1.0**, **Swashbuckle.AspNetCore 10.1.7** (OpenAPI via **Microsoft.OpenApi 2.x**).
- **APIs :** **Swashbuckle.AspNetCore 10.1.7** sur les services web ; **Microsoft.AspNetCore.OpenApi 8.0.22** là où il est référencé.
- **Auth :** **Microsoft.AspNetCore.Authentication.JwtBearer 8.0.22**.
- **JSON :** **Newtonsoft.Json 13.0.4** (là où le package est présent) et **System.Text.Json** (ASP.NET Core).
- **CI :** GitHub Actions, build Release de `API-Backend.sln`, puis les tests d’authentification. Pas de déploiement dans le workflow.

> Les versions **EF Core 10** et **ASP.NET Core 10** sur NuGet ciblent **.NET 10** ; ce dépôt reste sur **.NET 8** avec les versions **compatibles net8.0**.

---

## Structure du dépôt

```
API-Backend/
├── API-Backend.sln
├── Gateway/                 # Ocelot, Swagger centralisé
├── Gateway.Tests/
├── AuthentificationService/
├── AuthentificationService.Tests/
├── AssociationService/
├── MarketService/
├── BibliothequeService/
├── CafetariaService/
├── RessourcesPartagees/     # Types partagés
├── .github/workflows/ci.yml # Compilation sur main, develop et pull requests
└── LICENSE
```

---

## Prérequis et installation

1. **SDK :** [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) ou supérieur (pour compiler un projet `net8.0`).
2. **PostgreSQL 18** (local ou hébergé) et une chaîne de connexion Npgsql.
3. **Git**

```bash
git clone https://github.com/AzizAVERIBOU/Api-UqarLive.git
cd Api-UqarLive
dotnet restore API-Backend.sln
dotnet build API-Backend.sln -c Release
```

En cas de contrainte mémoire sur la machine de build, limitez le parallélisme MSBuild : `dotnet build API-Backend.sln -c Release -m:1`.

---

## Exécution locale

Exemple pour lancer un service depuis la racine du dépôt :

```bash
dotnet run --project Gateway
dotnet run --project AuthentificationService
```

Les ports viennent de `launchSettings.json` et de `UseUrls` dans `Program.cs`. La gateway écoute `PORT`, sinon `5000`. Le service d’authentification écoute `PORT`, sinon `5001`.

**Service d’authentification en ligne**

| Usage | Adresse |
|---|---|
| Santé | https://api-uqarlive.onrender.com/health |
| Swagger | https://api-uqarlive.onrender.com/swagger |
| Inscription | `POST /api/Authentification/Inscription` |
| Connexion | `POST /api/Authentification/Connexion` |
| Profil | `GET /api/Authentification/ObtenirProfilUtilisateur` |
| Vérifier un code | `GET /api/Authentification/VerifierUtilisateur/{codePermanent}` |

L’instance gratuite s’endort : le premier appel peut prendre près d’une minute.

---

## Configuration

### Chaîne de connexion

Les services lisent `ConnectionStrings:DefaultConnection`. En déploiement, posez-la dans la variable d’environnement `ConnectionStrings__DefaultConnection`. Ne commitez pas de mot de passe.

Format Npgsql :

```text
Host=localhost;Port=5432;Database=uqarlive;Username=postgres;Password=...;SSL Mode=Prefer
```

Pour un PostgreSQL Render, utilisez l’hôte interne et `SSL Mode=Require;Trust Server Certificate=true`. L’URL `postgresql://...` du tableau de bord n’est pas le format attendu par Npgsql.

### JWT (AuthentificationService)

La clé secrète se pose dans `JwtSettings__SecretKey`. La durée du jeton vient de `JwtSettings:ExpiryInMinutes` (60 dans `appsettings.json`). Ne commitez pas la clé.

### Appels vers l’authentification

Association, Marché et Bibliothèque lisent `Services:AuthService`. La valeur versionnée est `https://api-uqarlive.onrender.com/`. Pour un service d’authentification lancé sur la machine :

```text
Services__AuthService=http://localhost:5001/
```

Le chemin appelé est `api/Authentification/VerifierUtilisateur/{codePermanent}`.

### Gateway (Ocelot)

`Gateway/ocelot.json` publie `/auth/{everything}` vers `api-uqarlive.onrender.com` et `/biblio/{everything}` vers `uqarlive-bibliotheque.onrender.com`. Le service bibliothèque n'est pas encore en ligne : la passerelle ignore sa documentation tant qu'il ne répond pas. Association, Marché et Cafétéria ne sont pas routés.

---

## Docker

Chaque service a un Dockerfile. Le contexte de build est la **racine du dépôt**, parce que les Dockerfiles copient les `.csproj` depuis cette racine.

```bash
docker build -t uqarlive-gateway -f Gateway/Dockerfile .
docker build -t uqarlive-auth -f AuthentificationService/Dockerfile .
```

Il n’y a pas de `docker-compose` à la racine.

---

## Intégration continue

Deux branches long terme :

- `main` garde une version qui compile, celle qu'on peut montrer.
- `develop` reçoit le travail en cours.

Chaque tâche part de `develop` sur une branche `feature/...`, puis y revient par pull request. `main` est mise à jour depuis `develop`, aussi par pull request.

Le workflow [`.github/workflows/ci.yml`](.github/workflows/ci.yml) compile `API-Backend.sln` en Release avec le SDK .NET 8, puis lance `AuthentificationService.Tests`. Ces tests démarrent un PostgreSQL 18 jetable. Il tourne sur chaque push vers `main` ou `develop`, sur chaque pull request vers l'une de ces branches, et à la demande depuis l'onglet Actions. Un nouveau push sur la même référence annule le run encore en cours. Les paquets NuGet sont mis en cache.

Aucun job ne publie ni ne déploie les services. Le déploiement se fera plus tard, en dehors de ce workflow.

---

## Documentation des API

Le service d’authentification publie son contrat sur [https://api-uqarlive.onrender.com/swagger](https://api-uqarlive.onrender.com/swagger).

La passerelle reprend ce contrat dans sa propre page [http://localhost:5000/swagger](http://localhost:5000/swagger). Les chemins y sont ceux de la passerelle : `/api/Authentification/Connexion` devient `/auth/Connexion`. Un service absent est ignoré, la page reste ouverte.

---

## Migrations Entity Framework

Les migrations de l’authentification sont versionnées dans `AuthentificationService/Migrations`. Le service les applique au démarrage avec `Database.Migrate()`.

Les autres services n’ont pas encore de migrations versionnées.

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

*UQAR Live — APIs microservices .NET 8, gateway Ocelot, persistance PostgreSQL.*
