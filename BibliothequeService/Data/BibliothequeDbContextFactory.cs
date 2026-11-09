using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace BibliothequeService.Data
{
    /// <summary>
    /// Utilisée seulement par <c>dotnet ef</c> pour générer les migrations.
    /// Le démarrage de l'application lit la vraie chaîne dans la configuration.
    /// </summary>
    public class BibliothequeDbContextFactory : IDesignTimeDbContextFactory<BibliothequeDbContext>
    {
        public BibliothequeDbContext CreateDbContext(string[] args)
        {
            var options = new DbContextOptionsBuilder<BibliothequeDbContext>();
            BibliothequeDbContext.UtiliserPostgres(options, "Host=localhost;Port=5432;Database=uqarlive");
            return new BibliothequeDbContext(options.Options);
        }
    }
}
