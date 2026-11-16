using System.ComponentModel.DataAnnotations;

namespace CafetariaService.Models
{
    public class DemandeMenu
    {
        [Required]
        public DateOnly Date { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        public List<int> PlatIds { get; set; } = new();
    }
}
