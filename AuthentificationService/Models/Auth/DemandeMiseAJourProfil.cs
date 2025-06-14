namespace AuthentificationService.Models.Auth
{
    public class DemandeMiseAJourProfil
    {
        public string Email { get; set; }
        public double Telephone { get; set; }
        public string Nom { get; set; }
        public string Prenom { get; set; }
        public string Programme { get; set; }
        public string Faculte { get; set; }
        public int? AnneeDebut { get; set; }
        public int? AnneeFin { get; set; }
        public AdresseMiseAJour Adresse { get; set; }
    }
} 
