# Cafétéria

Catalogue, menu du jour et horaires de la semaine. Les lectures et les écritures sont ouvertes. Ce service n'appelle pas l'authentification.

Public : [https://uqarlive-cafetaria.onrender.com](https://uqarlive-cafetaria.onrender.com)

| | |
|---|---|
| Santé | [/health](https://uqarlive-cafetaria.onrender.com/health) |
| Documentation | [/swagger](https://uqarlive-cafetaria.onrender.com/swagger) |
| Passerelle | `/cafete/…` |
| Port local | `5005`, ou `PORT` |
| Historique EF | `__CafetariaMigrationsHistory` |

## Lancer

Depuis la racine du dépôt, avec `ConnectionStrings__DefaultConnection` :

```bash
dotnet run --project CafetariaService
```

Image, contexte = racine du dépôt :

```bash
docker build -t uqarlive-cafetaria -f CafetariaService/Dockerfile .
```

Le prix d'un plat va de 0 à 1 000. Le menu du jour suit le calendrier du Québec (`America/Toronto`). Une date de menu est unique. Les plats du menu sont des identifiants déjà présents dans le catalogue. Les horaires vont du lundi (1) au vendredi (5). `PUT /horaires` remplace toute la semaine.

## Routes

Le contrôleur est `api/Cafetaria`. Par la passerelle, le préfixe devient `/cafete`.

| Méthode | Chemin | Rôle |
|---|---|---|
| GET | `/menuitems` | Liste le catalogue |
| POST | `/menuitems` | Ajoute un plat |
| PUT | `/menuitems/{id}` | Met à jour un plat |
| DELETE | `/menuitems/{id}` | Retire un plat |
| GET | `/menu/aujourdhui` | Menu actif du jour |
| POST | `/menu` | Crée le menu d'une date |
| GET | `/horaires` | Horaires de la semaine |
| PUT | `/horaires` | Remplace les horaires |
| GET | `/horaires/{jour}` | Horaire d'un jour |

Les tests sont dans `CafetariaService.Tests`. Ils démarrent un PostgreSQL 18 jetable.

Le détail du dépôt est dans le [README racine](../README.md).
