using RessourcesPartagees.Enumerations;

namespace AuthentificationService.Models.Auth;

public class MessageApi
{
    public string Message { get; set; }
}

public class DeconnexionApi
{
    public string Message { get; set; }
    public string CodePermanent { get; set; }
}

public class VerificationUtilisateurApi
{
    public string Message { get; set; }
    public string CodePermanent { get; set; }
    public string Nom { get; set; }
    public string Prenom { get; set; }
    public string Email { get; set; }
    public RoleUtilisateur Role { get; set; }
    public bool Existe { get; set; }
}

public class ProfilApi
{
    public string CodePermanent { get; set; }
    public string Email { get; set; }
    public string Nom { get; set; }
    public string Prenom { get; set; }
    public string Telephone { get; set; }
    public RoleUtilisateur Role { get; set; }
    public string Programme { get; set; }
    public string Faculte { get; set; }
    public int AnneeDebut { get; set; }
    public int AnneeFin { get; set; }
    public AdresseMiseAJour Adresse { get; set; }
    public DateTime DateInscription { get; set; }
    public DateTime DerniereConnexion { get; set; }
    public bool EstActif { get; set; }
}
