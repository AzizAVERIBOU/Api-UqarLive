namespace AssociationService.Models
{
    public class RepondreDemandeRequest
    {
        public int DemandeId { get; set; }
        [System.ComponentModel.DataAnnotations.Required]
        [System.ComponentModel.DataAnnotations.StringLength(20)]
        public string CodePermanentRepondant { get; set; } = string.Empty;
        public bool Accepter { get; set; }
        public string? Reponse { get; set; }
    }
} 