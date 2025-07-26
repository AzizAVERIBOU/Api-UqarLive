using System.ComponentModel.DataAnnotations;

namespace CafetariaService.Models
{
    public class Menu
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public DateTime Date { get; set; }

        public List<MenuItem> PlatsDuJour { get; set; } = new();

        public bool EstActif { get; set; } = true;

        public string? Description { get; set; }
    }
} 