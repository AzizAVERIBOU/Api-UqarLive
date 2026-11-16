using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace MarketService.Data
{
    /// <summary>
    /// Utilisée seulement par <c>dotnet ef</c> pour générer les migrations.
    /// Le démarrage de l'application lit la vraie chaîne dans la configuration.
    /// </summary>
    public class MarketDbContextFactory : IDesignTimeDbContextFactory<MarketDbContext>
    {
        public MarketDbContext CreateDbContext(string[] args)
        {
            var options = new DbContextOptionsBuilder<MarketDbContext>();
            MarketDbContext.UtiliserPostgres(options, "Host=localhost;Port=5432;Database=uqarlive");
            return new MarketDbContext(options.Options);
        }
    }
}
