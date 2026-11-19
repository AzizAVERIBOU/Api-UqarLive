using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AssociationService.Data
{
    /// <summary>
    /// Utilisée seulement par <c>dotnet ef</c> pour générer les migrations.
    /// Le démarrage de l'application lit la vraie chaîne dans la configuration.
    /// </summary>
    public class AssociationDbContextFactory : IDesignTimeDbContextFactory<AssociationDbContext>
    {
        public AssociationDbContext CreateDbContext(string[] args)
        {
            var options = new DbContextOptionsBuilder<AssociationDbContext>();
            AssociationDbContext.UtiliserPostgres(options, "Host=localhost;Port=5432;Database=uqarlive");
            return new AssociationDbContext(options.Options);
        }
    }
}
