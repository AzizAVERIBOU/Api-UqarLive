# Association

Associations étudiantes, adhésions et événements. Créer une association exige un code permanent qui correspond à un compte actif. La création force le statut En attente et ajoute le créateur comme président.

Public : [https://uqarlive-association.onrender.com](https://uqarlive-association.onrender.com)

| | |
|---|---|
| Santé | [/health](https://uqarlive-association.onrender.com/health) |
| Documentation | [/swagger](https://uqarlive-association.onrender.com/swagger) |
| Passerelle | `/asso/…` |
| Port local | `5002`, ou `PORT` |
| Historique EF | `__AssociationMigrationsHistory` |

## Lancer

Depuis la racine du dépôt, avec `ConnectionStrings__DefaultConnection` :

```bash
dotnet run --project AssociationService
```

Image, contexte = racine du dépôt :

```bash
docker build -t uqarlive-association -f AssociationService/Dockerfile .
```

L'appel vers l'authentification lit `Services:AuthService`. La valeur versionnée est `https://api-uqarlive.onrender.com/`. Un code inconnu répond 400. Une panne répond 500. Le délai est de 90 secondes. Cet appel ne voit pas de session.

Le nom est unique sans tenir compte de la casse. Les descriptions sont limitées à 2 000 caractères, la motivation à 1 000, les codes permanents à 20. Le lieu d'un événement est une adresse postale. Les dates d'événement sont enregistrées en UTC.

## Routes

Le contrôleur est `api/Association`. Les noms français sont conservés. Par la passerelle, le préfixe devient `/asso`.

| Méthode | Chemin | Rôle |
|---|---|---|
| GET | `/test` | Le contrôleur répond |
| GET | `/ObtenirAssociations` | Liste les associations |
| GET | `/ObtenirAssociation/{id}` | Lit une association |
| POST | `/CreerAssociation` | Crée une association |
| PUT | `/ModifierAssociation/{id}` | Met à jour une association |
| DELETE | `/SupprimerAssociation/{id}` | Supprime une association et ses éléments liés |
| GET | `/ObtenirMembres/{id}` | Liste les membres |
| POST | `/DemanderAdhesion/{id}` | Dépose une demande |
| POST | `/AjouterMembre/{id}` | Accepte ou refuse une demande |
| PUT | `/ModifierRoleMembre/{id}/{codePermanent}` | Change un rôle |
| DELETE | `/RetirerMembre/{id}/{codePermanent}` | Retire un membre |
| GET | `/ObtenirEvenements/{id}` | Liste les événements |
| POST | `/CreerEvenement/{id}` | Crée un événement planifié |
| PUT | `/ModifierEvenement/{id}/{evenementId}` | Met à jour un événement qui n'a pas commencé |
| DELETE | `/SupprimerEvenement/{id}/{evenementId}` | Supprime un événement qui n'a pas commencé |
| GET | `/ObtenirDemandesAdhesion/{id}` | Liste les demandes en attente |

Les énumérations sont des nombres dans le JSON. En attente vaut 2 pour une association, 0 pour une demande.

Les tests sont dans `AssociationService.Tests`. Ils démarrent un PostgreSQL 18 jetable.

Le détail du dépôt est dans le [README racine](../README.md).
