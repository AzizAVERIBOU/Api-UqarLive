# Ressources partagées

Bibliothèque de classes utilisée par plusieurs services. Ce projet n'écoute aucun port et n'a pas de base ni de Dockerfile.

## Types

| Type | Usage |
|---|---|
| `Adresse` | Adresse d'un compte, avec identifiant. L'appartement est facultatif et sans longueur maximale. Le code permanent lié accepte 450 caractères |
| `AdressePostale` | Adresse sans identifiant, possédée par l'événement. L'appartement est facultatif, au plus 50 caractères. Les autres champs sont obligatoires |
| `RoleUtilisateur` | Nombres stables : 0 Étudiant, 1 Personnel, 2 Admin |

Les services référencent ce projet. Une image Docker qui en dépend doit copier `RessourcesPartagees.csproj` depuis la racine du dépôt, comme le fait Association.

Le détail du dépôt est dans le [README racine](../README.md).
