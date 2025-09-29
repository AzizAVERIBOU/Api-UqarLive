# Script de mise à jour du dépôt GitHub
# UQAR Live - API Backend Microservices

Write-Host "Mise à jour du dépôt GitHub UQAR Live..." -ForegroundColor Green

# Vérifier le statut Git
Write-Host "Vérification du statut Git..." -ForegroundColor Yellow
git status

# Ajouter tous les fichiers modifiés
Write-Host "Ajout des fichiers modifiés..." -ForegroundColor Yellow
git add .

# Créer un commit avec un message descriptif
$commitMessage = "Mise à jour documentation complète + Licence MIT

Améliorations:
- Documentation technique détaillée des packages NuGet
- Organisation en sections thématiques
- Ajout de la licence MIT (EN/FR)
- Mise à jour des informations de contact
- Structure README optimisée

Technologies:
- .NET 8.0 + Entity Framework Core 9.0.6
- Ocelot Gateway + Swagger/OpenAPI
- Docker + Azure App Service
- Architecture microservices complète"

Write-Host "Création du commit..." -ForegroundColor Yellow
git commit -m $commitMessage

# Pousser vers GitHub
Write-Host "Poussée vers GitHub..." -ForegroundColor Yellow
git push origin main

Write-Host "Mise à jour terminée avec succès!" -ForegroundColor Green
Write-Host "Dépôt: https://github.com/AzizAVERIBOU/Api-UqarLive.git" -ForegroundColor Cyan
