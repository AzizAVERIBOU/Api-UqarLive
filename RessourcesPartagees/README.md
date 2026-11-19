# Ressources partagées

Bibliothèque de classes utilisée par plusieurs services. Ce projet n'écoute aucun port et n'a pas de base ni de Dockerfile.

## Types

| Type | Usage |
|---|---|
| `Adresse` | Adresse d'un compte, côté authentification |
| `AdressePostale` | Lieu d'un événement. Numéro, rue, ville, province et code postal sont obligatoires. L'appartement est facultatif |
| `RoleUtilisateur` | 0 Étudiant, 1 Personnel, 2 Admin |

Les services référencent ce projet. Une image Docker qui en dépend doit copier `RessourcesPartagees.csproj` depuis la racine du dépôt, comme le fait Association.

Le détail du dépôt est dans le [README racine](../README.md).
