using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AuthentificationService.Data
{
    /// <summary>
    /// Utilisée seulement par <c>dotnet ef</c> pour générer les migrations.
    /// Le démarrage de l'application lit la vraie chaîne dans la configuration.
    /// </summary>
    public class AuthentificationDbContextFactory : IDesignTimeDbContextFactory<AuthentificationDbContext>
    {
        public AuthentificationDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<AuthentificationDbContext>();
            optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=uqarlive");

            return new AuthentificationDbContext(optionsBuilder.Options);
        }
    }
}
