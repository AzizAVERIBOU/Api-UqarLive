# Authentification

Comptes, connexion et jetons JWT. L'inscription publique enregistre toujours le rôle Étudiant.

Public : [https://api-uqarlive.onrender.com](https://api-uqarlive.onrender.com)

| | |
|---|---|
| Santé | [/health](https://api-uqarlive.onrender.com/health) |
| Documentation | [/swagger](https://api-uqarlive.onrender.com/swagger) |
| Passerelle | `/auth/…` |
| Port local | `5001`, ou `PORT` |
| Historique EF | `__EFMigrationsHistory` |

## Lancer

Depuis la racine du dépôt, avec `ConnectionStrings__DefaultConnection` :

```bash
dotnet run --project AuthentificationService
```

Image, contexte = racine du dépôt :

```bash
docker build -t uqarlive-auth -f AuthentificationService/Dockerfile .
```

La clé JWT se pose dans `JwtSettings__SecretKey`. La durée vient de `JwtSettings:ExpiryInMinutes`. Ne commitez pas la clé ni la chaîne de connexion.

## Routes

Le contrôleur est `api/Authentification`. Par la passerelle, le préfixe devient `/auth`.

| Méthode | Chemin | Rôle |
|---|---|---|
| GET | `/test` | Le contrôleur répond |
| POST | `/Inscription` | Crée un compte Étudiant |
| POST | `/Connexion` | Retourne un jeton |
| POST | `/RafraichirToken` | Renouvelle le jeton |
| POST | `/Deconnexion` | Clôt la session |
| GET | `/ObtenirProfilUtilisateur` | Profil du jeton |
| PUT | `/MettreAJourProfilUtilisateur` | Met à jour le profil |
| POST | `/ChangerMotDePasse` | Change le mot de passe |
| GET | `/VerifierUtilisateur/{codePermanent}` | 200 si le compte existe et est actif, 404 sinon |

`VerifierUtilisateur` ne dit pas si une session est ouverte. Le rôle dans la réponse est un nombre : 0 Étudiant, 1 Personnel, 2 Admin.

Les tests sont dans `AuthentificationService.Tests`. Ils démarrent un PostgreSQL 18 jetable.

Le détail du dépôt est dans le [README racine](../README.md).
