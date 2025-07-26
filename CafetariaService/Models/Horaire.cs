using System.ComponentModel.DataAnnotations;

namespace CafetariaService.Models
{
    public class Horaire
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public JourSemaine JourDeLaSemaine { get; set; } = JourSemaine.Lundi; // Utilisation de l'énumération

        [Required]
        public TimeSpan HeureOuverture { get; set; } = new TimeSpan(8, 0, 0);

        [Required]
        public TimeSpan HeureFermeture { get; set; } = new TimeSpan(16, 0, 0);
    }
} 