using System.ComponentModel.DataAnnotations;

namespace CafetariaService.Models
{
    public class MenuItem
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Nom { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        [Range(0, 1000)]
        public decimal Prix { get; set; }

        public bool Disponible { get; set; } = true;
    }
} 