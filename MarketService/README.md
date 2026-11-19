# Marché

Annonces entre étudiants. Publier une annonce exige un code permanent qui correspond à un compte actif.

Public : [https://uqarlive-marche.onrender.com](https://uqarlive-marche.onrender.com)

| | |
|---|---|
| Santé | [/health](https://uqarlive-marche.onrender.com/health) |
| Documentation | [/swagger](https://uqarlive-marche.onrender.com/swagger) |
| Passerelle | `/marche/…` |
| Port local | `5003`, ou `PORT` |
| Historique EF | `__MarcheMigrationsHistory` |

## Lancer

Depuis la racine du dépôt, avec `ConnectionStrings__DefaultConnection` :

```bash
dotnet run --project MarketService
```

Image, contexte = racine du dépôt :

```bash
docker build -t uqarlive-marche -f MarketService/Dockerfile .
```

L'appel vers l'authentification lit `Services:AuthService`. La valeur versionnée est `https://api-uqarlive.onrender.com/`. Un code inconnu répond 400. Une panne répond 500. Le délai est de 90 secondes.

Le prix va de 0 à 10 000. La création force le statut Disponible. La catégorie doit déjà exister.

## Routes

Le contrôleur est `api/Market`. Par la passerelle, le préfixe devient `/marche`.

| Méthode | Chemin | Rôle |
|---|---|---|
| GET | `/produits` | Liste les annonces |
| GET | `/produits/{id}` | Lit une annonce |
| POST | `/produits` | Publie une annonce |
| PUT | `/produits/{id}` | Met à jour une annonce |
| PUT | `/produits/{id}/statut` | Change le statut |
| DELETE | `/produits/{id}` | Retire une annonce |
| GET | `/categories` | Liste les catégories |
| GET | `/categories/{id}` | Lit une catégorie |
| POST | `/categories` | Ajoute une catégorie |
| PUT | `/categories/{id}` | Met à jour une catégorie |

Les statuts et les modes de paiement sont des nombres dans le JSON. En base, ils sont des textes courts.

Les tests sont dans `MarketService.Tests`. Ils démarrent un PostgreSQL 18 jetable.

Le détail du dépôt est dans le [README racine](../README.md).
