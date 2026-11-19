# Bibliothèque

Salles et réservations. Créer une réservation exige un code permanent qui correspond à un compte actif.

Public : [https://uqarlive-bibliotheque.onrender.com](https://uqarlive-bibliotheque.onrender.com)

| | |
|---|---|
| Santé | [/health](https://uqarlive-bibliotheque.onrender.com/health) |
| Documentation | [/swagger](https://uqarlive-bibliotheque.onrender.com/swagger) |
| Passerelle | `/biblio/…` |
| Port local | `5004`, ou `PORT` |
| Historique EF | `__BibliothequeMigrationsHistory` |

## Lancer

Depuis la racine du dépôt, avec `ConnectionStrings__DefaultConnection` :

```bash
dotnet run --project BibliothequeService
```

Image, contexte = racine du dépôt :

```bash
docker build -t uqarlive-bibliotheque -f BibliothequeService/Dockerfile .
```

L'appel vers l'authentification lit `Services:AuthService`. La valeur versionnée est `https://api-uqarlive.onrender.com/`. Le chemin vérifié est `api/Authentification/VerifierUtilisateur/{codePermanent}`. Un code inconnu répond 400. Une panne répond 500. Le délai est de 90 secondes.

## Routes

Le contrôleur est `api/Bibliotheque`. Par la passerelle, le préfixe devient `/biblio`.

| Méthode | Chemin | Rôle |
|---|---|---|
| GET | `/salles` | Liste les salles |
| GET | `/salles/{id}` | Lit une salle |
| POST | `/salles` | Ajoute une salle |
| PUT | `/salles/{id}` | Met à jour une salle |
| DELETE | `/salles/{id}` | Retire une salle |
| GET | `/salles/disponibles` | Salles libres sur un créneau |
| GET | `/reservations` | Liste les réservations |
| GET | `/reservations/{id}` | Lit une réservation |
| POST | `/reservations` | Crée une réservation |
| PUT | `/reservations/{id}` | Met à jour une réservation |
| DELETE | `/reservations/{id}` | Annule une réservation |
| GET | `/heures` | Créneaux proposés |

Les tests sont dans `BibliothequeService.Tests`. Ils démarrent un PostgreSQL 18 jetable.

Le détail du dépôt est dans le [README racine](../README.md).
