using Microsoft.EntityFrameworkCore;
using CafetariaService.Models;

namespace CafetariaService.Data
{
    public class CafetariaDbContext : DbContext
    {
        public CafetariaDbContext(DbContextOptions<CafetariaDbContext> options) : base(options) { }

        public DbSet<MenuItem> MenuItems { get; set; }
        public DbSet<Menu> Menus { get; set; }
        public DbSet<Horaire> Horaires { get; set; }
    }
} 