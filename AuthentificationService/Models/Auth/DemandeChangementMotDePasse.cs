namespace AuthentificationService.Models.Auth
{
    public class DemandeChangementMotDePasse
    {
        public string AncienMotDePasse { get; set; }
        public string NouveauMotDePasse { get; set; }
        public string ConfirmationMotDePasse { get; set; }
    }
} 