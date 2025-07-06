namespace AssociationService.Models
{
    public class RepondreDemandeRequest
    {
        public int DemandeId { get; set; }
        public string CodePermanentRepondant { get; set; }
        public bool Accepter { get; set; }
        public string? Reponse { get; set; }
    }
} 