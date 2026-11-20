# Passerelle

Point d'entrée d'UQAR Live. Ocelot route les appels. Swagger affiche un document par service et réécrit ses chemins.

Public : [https://uqarlive-gateway.onrender.com](https://uqarlive-gateway.onrender.com)

| | |
|---|---|
| Santé | [/health](https://uqarlive-gateway.onrender.com/health) |
| Documentation | [/swagger](https://uqarlive-gateway.onrender.com/swagger) |
| Port local | `5000`, ou `PORT` |
| Base de données | aucune |

## Lancer

Depuis la racine du dépôt :

```bash
dotnet run --project Gateway
```

Image, contexte = racine du dépôt :

```bash
docker build -t uqarlive-gateway -f Gateway/Dockerfile .
```

## Routes

| Préfixe | Service |
|---|---|
| `/auth/{everything}` | [Authentification](../AuthentificationService/README.md) → `api-uqarlive.onrender.com` |
| `/biblio/{everything}` | [Bibliothèque](../BibliothequeService/README.md) → `uqarlive-bibliotheque.onrender.com` |
| `/marche/{everything}` | [Marché](../MarketService/README.md) → `uqarlive-marche.onrender.com` |
| `/cafete/{everything}` | [Cafétéria](../CafetariaService/README.md) → `uqarlive-cafetaria.onrender.com` |
| `/asso/{everything}` | [Association](../AssociationService/README.md) → `uqarlive-association.onrender.com` |

Les méthodes publiées sont GET, POST, PUT et DELETE. Le fichier est `ocelot.json`.

Pas de modèle ni de base. Le port est `PORT`, sinon `5000`. Aucune redirection HTTPS. `/`, `/health` et `/swagger` sont répondues ici et ne partent pas vers un service.

`GET /` liste ces préfixes. `GET /swagger` propose un document par service : `/swagger/auth.json`, `/swagger/biblio.json`, `/swagger/marche.json`, `/swagger/cafete.json` et `/swagger/asso.json`. Chaque document est lu sur le `/swagger/v1/swagger.json` du service, avec un délai de 60 secondes, et ne garde que les chemins de son contrôleur. Le `/health` du service n'y apparaît pas. « Try it out » appelle cette passerelle. Un service injoignable garde sa page, avec une liste de chemins vide. Une clé qui n'est pas un préfixe répond 404.

Les tests de `Gateway.Tests` réécrivent ces documents en mémoire. Ils ne démarrent pas PostgreSQL.

Le détail du dépôt est dans le [README racine](../README.md).
