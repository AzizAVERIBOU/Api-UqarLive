using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AuthentificationService.Models;
using Microsoft.IdentityModel.Tokens;

namespace AuthentificationService.Models.Auth
{
    public class JwtToken
    {
        private readonly IConfiguration _configuration;

        public JwtToken(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public ReponseToken GenererToken(Utilisateur utilisateur)
        {
            var cle = Encoding.ASCII.GetBytes(_configuration["JwtSettings:SecretKey"]);
            var gestionnaireToken = new JwtSecurityTokenHandler();
            
            var dateExpiration = DateTime.UtcNow.AddHours(1);
            
            var descripteurToken = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, utilisateur.CodePermanent),
                    new Claim(ClaimTypes.Email, utilisateur.Email),
                    new Claim(ClaimTypes.Role, utilisateur.Role.ToString())
                }),
                Expires = dateExpiration,
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(cle),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = gestionnaireToken.CreateToken(descripteurToken);
            var tokenString = gestionnaireToken.WriteToken(token);

            return new ReponseToken
            {
                Token = tokenString,
                DateExpiration = dateExpiration,
                Message = "Token généré avec succès",
                Utilisateur = new ReponseToken.UtilisateurInfo
                {
                    CodePermanent = utilisateur.CodePermanent,
                    Email = utilisateur.Email,
                    Nom = utilisateur.Nom,
                    Prenom = utilisateur.Prenom,
                    Role = utilisateur.Role.ToString()
                }
            };
        }
    }
} 