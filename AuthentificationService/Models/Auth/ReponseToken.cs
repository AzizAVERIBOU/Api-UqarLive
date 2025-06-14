namespace AuthentificationService.Models.Auth
{
    public class ReponseToken
    {
        public string Token { get; set; }
        public DateTime DateExpiration { get; set; }
        public string Type { get; set; } = "Bearer";
        public string Message { get; set; }
        public UtilisateurInfo Utilisateur { get; set; }

        public class UtilisateurInfo
        {
            public string CodePermanent { get; set; }
            public string Email { get; set; }
            public string Nom { get; set; }
            public string Prenom { get; set; }
            public string Role { get; set; }
        }
    }
} 