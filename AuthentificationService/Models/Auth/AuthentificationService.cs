using System.Security.Cryptography;
using System.Text;
using AuthentificationService.Models;
using AuthentificationService.Data;
using Microsoft.EntityFrameworkCore;

namespace AuthentificationService.Models.Auth
{
    public class ServiceAuthentification
    {
        private readonly AuthentificationDbContext _context;

        public ServiceAuthentification(AuthentificationDbContext context)
        {
            _context = context;
        }

        public (byte[] hash, byte[] salt) HasherMotDePasse(string motDePasse)
        {
            using var hmac = new HMACSHA512();
            var motDePasseHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(motDePasse));
            var motDePasseSalt = hmac.Key;
            return (motDePasseHash, motDePasseSalt);
        }

        public bool VerifierMotDePasse(string motDePasse, byte[] motDePasseHash, byte[] motDePasseSalt)
        {
            using var hmac = new HMACSHA512(motDePasseSalt);
            var hashCalcule = hmac.ComputeHash(Encoding.UTF8.GetBytes(motDePasse));
            return hashCalcule.SequenceEqual(motDePasseHash);
        }

        public async Task<Utilisateur> AuthentifierUtilisateur(string codePermanent, string motDePasse)
        {
            var utilisateur = await _context.Utilisateurs.FirstOrDefaultAsync(u => u.CodePermanent == codePermanent);
            if (utilisateur == null)
                return null;

            if (!VerifierMotDePasse(motDePasse, utilisateur.MotDePasseHash, utilisateur.MotDePasseSalt))
                return null;

            return utilisateur;
        }
    }
} 