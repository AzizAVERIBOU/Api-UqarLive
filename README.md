# API Backend - Architecture Microservices

Ce projet est une architecture microservices développée en .NET, composée de plusieurs services indépendants.

## Services

- **AuthentificationService** : Gestion de l'authentification des utilisateurs
- **NotificationService** : Gestion des notifications
- **MarketService** : Gestion du marché/boutique
- **BibliothequeService** : Gestion de la bibliothèque
- **CafetariaService** : Gestion de la cafétéria
- **AssociationService** : Gestion des associations
- **Gateway** : Point d'entrée unique pour tous les services

## Structure du Projet

Chaque service suit une architecture MVC avec :
- `Models/` : Classes de données et entités
- `Controllers/` : Gestionnaires de requêtes HTTP

## Prérequis

- .NET 8.0 SDK ou supérieur
- Visual Studio 2022 ou Visual Studio Code
- Git

## Installation

1. Cloner le repository :
```bash
git clone [URL_DU_REPO]
```

2. Restaurer les dépendances :
```bash
dotnet restore
```

3. Lancer les services :
```bash
dotnet run --project [NOM_DU_SERVICE]
```

## Développement

Pour ajouter un nouveau service :
1. Créer un nouveau projet dans le dossier approprié
2. Ajouter les dossiers Models et Controllers
3. Configurer les dépendances nécessaires
4. Ajouter le projet à la solution principale

## Contribution

1. Créer une nouvelle branche pour votre fonctionnalité
2. Faire vos modifications
3. Créer une Pull Request

## Licence

je n'ai pas de licence a jour pour le moment ! 

