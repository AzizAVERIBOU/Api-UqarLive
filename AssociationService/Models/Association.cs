using AssociationService.Models.Enumerations;

namespace AssociationService.Models
{
    public class Association
    {
        public int Id { get; set; }
        public string Nom { get; set; }
        public string Description { get; set; }
        public CategorieAssociation Categorie { get; set; }
        public Faculte Faculte { get; set; }
        public string CodePermanentCreateur { get; set; }
        public List<string> Objectifs { get; set; }
        public List<string> Activites { get; set; }
        public DateTime DateCreation { get; set; }
        public StatutAssociation Statut { get; set; }
        public List<Membre> Membres { get; set; }
        public List<Evenement> Evenements { get; set; }
        public List<DemandeAdhesion> DemandesAdhesion { get; set; }
    }
} 