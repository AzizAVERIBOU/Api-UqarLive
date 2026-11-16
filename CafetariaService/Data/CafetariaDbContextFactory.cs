using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CafetariaService.Data
{
    /// <summary>
    /// Utilisée seulement par <c>dotnet ef</c> pour générer les migrations.
    /// Le démarrage de l'application lit la vraie chaîne dans la configuration.
    /// </summary>
    public class CafetariaDbContextFactory : IDesignTimeDbContextFactory<CafetariaDbContext>
    {
        public CafetariaDbContext CreateDbContext(string[] args)
        {
            var options = new DbContextOptionsBuilder<CafetariaDbContext>();
            CafetariaDbContext.UtiliserPostgres(options, "Host=localhost;Port=5432;Database=uqarlive");
            return new CafetariaDbContext(options.Options);
        }
    }
}
