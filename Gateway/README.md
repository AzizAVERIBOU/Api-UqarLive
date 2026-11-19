# Passerelle

Point d'entrée d'UQAR Live. Ocelot route les appels et le Swagger central réécrit les chemins des services.

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

`GET /` liste ces préfixes. `GET /swagger/passerelle.json` assemble les documents des services joignables. Un service endormi ou injoignable est ignoré, la page reste ouverte. « Try it out » appelle cette passerelle.

Les tests de `Gateway.Tests` réécrivent ces documents en mémoire. Ils ne démarrent pas PostgreSQL.

Le détail du dépôt est dans le [README racine](../README.md).
