namespace AuthentificationService.Models
{
    public class ReponseErreur
    {
        public string Message { get; set; }
        public string Code { get; set; }
        public List<string> Details { get; set; }
    }
} 