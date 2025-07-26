using System.Collections.Generic;

namespace CafetariaService.Models
{
    public class HoraireSemaine
    {
        // Un dictionnaire : clé = JourSemaine, valeur = Horaire du jour
        public Dictionary<JourSemaine, Horaire> Horaires { get; set; } = new();

        // Ajoute ou met à jour l'horaire d'un jour (empêche les doublons)
        public void AjouterOuMettreAJourHoraire(Horaire horaire)
        {
            Horaires[horaire.JourDeLaSemaine] = horaire;
        }

        // Récupère l'horaire d'un jour
        public Horaire? GetHoraireDuJour(JourSemaine jour)
        {
            return Horaires.TryGetValue(jour, out var horaire) ? horaire : null;
        }
    }
} 