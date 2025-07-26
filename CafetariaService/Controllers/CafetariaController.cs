using Microsoft.AspNetCore.Mvc;
using CafetariaService.Models;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.EntityFrameworkCore;
using CafetariaService.Data;

namespace CafetariaService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [ApiExplorerSettings(GroupName = "v1")]
    public class CafetariaController : ControllerBase
    {
        private readonly CafetariaDbContext _context;

        public CafetariaController(CafetariaDbContext context)
        {
            _context = context;
        }

        // --- Gestion des plats disponibles (catalogue) ---
        [HttpGet("menuitems")]
        public async Task<ActionResult<IEnumerable<MenuItem>>> GetMenuItems()
        {
            return Ok(await _context.MenuItems.ToListAsync());
        }

        [HttpPost("menuitems")]
        public async Task<ActionResult<MenuItem>> AddMenuItem([FromBody] MenuItem item)
        {
            _context.MenuItems.Add(item);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetMenuItems), new { id = item.Id }, item);
        }

        [HttpPut("menuitems/{id}")]
        public async Task<IActionResult> UpdateMenuItem(int id, [FromBody] MenuItem item)
        {
            var existing = await _context.MenuItems.FindAsync(id);
            if (existing == null) return NotFound();
            
            existing.Nom = item.Nom;
            existing.Description = item.Description;
            existing.Prix = item.Prix;
            existing.Disponible = item.Disponible;
            
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("menuitems/{id}")]
        public async Task<IActionResult> DeleteMenuItem(int id)
        {
            var item = await _context.MenuItems.FindAsync(id);
            if (item == null) return NotFound();
            
            _context.MenuItems.Remove(item);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // --- Gestion du menu du jour ---
        [HttpGet("menu/aujourdhui")]
        public async Task<ActionResult<Menu>> GetMenuDuJour()
        {
            var aujourdhui = DateTime.Today;
            var menu = await _context.Menus
                .Include(m => m.PlatsDuJour)
                .FirstOrDefaultAsync(m => m.Date.Date == aujourdhui && m.EstActif);
            
            if (menu == null)
            {
                return NotFound(new { message = "Aucun menu du jour trouvé" });
            }
            
            return Ok(menu);
        }

        [HttpPost("menu")]
        public async Task<ActionResult<Menu>> CreerMenuDuJour([FromBody] Menu menu)
        {
            // Vérifier s'il existe déjà un menu pour cette date
            var menuExistant = await _context.Menus
                .FirstOrDefaultAsync(m => m.Date.Date == menu.Date.Date);
            
            if (menuExistant != null)
            {
                return BadRequest(new { message = "Un menu existe déjà pour cette date" });
            }
            
            _context.Menus.Add(menu);
            await _context.SaveChangesAsync();
            
            return CreatedAtAction(nameof(GetMenuDuJour), new { id = menu.Id }, menu);
        }

        // --- Gestion des horaires ---
        [HttpGet("horaires")]
        public async Task<ActionResult<HoraireSemaine>> GetHorairesSemaine()
        {
            var horaires = await _context.Horaires.ToListAsync();
            var horaireSemaine = new HoraireSemaine();
            
            foreach (var horaire in horaires)
            {
                horaireSemaine.AjouterOuMettreAJourHoraire(horaire);
            }
            
            return Ok(horaireSemaine);
        }

        [HttpPut("horaires")]
        public async Task<IActionResult> UpdateHorairesSemaine([FromBody] List<Horaire> horaires)
        {
            // Supprimer tous les horaires existants
            _context.Horaires.RemoveRange(_context.Horaires);
            
            // Ajouter les nouveaux horaires
            _context.Horaires.AddRange(horaires);
            await _context.SaveChangesAsync();
            
            return NoContent();
        }

        [HttpGet("horaires/{jour}")]
        public async Task<ActionResult<Horaire>> GetHoraireDuJour(JourSemaine jour)
        {
            var horaire = await _context.Horaires
                .FirstOrDefaultAsync(h => h.JourDeLaSemaine == jour);
                
            if (horaire == null) return NotFound();
            return Ok(horaire);
        }
    }
} 