namespace AuthentificationService.Models
{
    public class ReponseAuthentification
    {
        public string Token { get; set; }
        public string RefreshToken { get; set; }
        public DateTime ExpirationToken { get; set; }
        public Utilisateur Utilisateur { get; set; }
    }
} 