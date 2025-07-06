using AssociationService.Models.Enumerations;

namespace AssociationService.Models
{
    /// <summary>
    /// Requête pour modifier le rôle d'un membre
    /// </summary>
    public class ModifierRoleRequest
    {
        /// <summary>
        /// Code permanent de l'utilisateur qui effectue la modification du rôle
        /// <para>Doit être Président ou Vice-président de l'association</para>
        /// <para>Ne peut pas être le même que le membre dont on modifie le rôle</para>
        /// </summary>
        /// <example>TREM5678</example>
        public string CodePermanentModifiant { get; set; }

        /// <summary>
        /// Nouveau rôle à attribuer au membre
        /// <para>0 = President</para>
        /// <para>1 = VicePresident</para>
        /// <para>2 = Tresorier</para>
        /// <para>3 = Secretaire</para>
        /// <para>4 = Participant</para>
        /// </summary>
        /// <example>1</example>
        public RoleMembre NouveauRole { get; set; }
    }
} 