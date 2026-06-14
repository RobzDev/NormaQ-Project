using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace NormaQ.ViewModels
{
    public class FirmarVersionViewModel
    {
        [Required]
        public int VersionId { get; set; }
        
        [Required]
        public string Accion { get; set; } = string.Empty; // "Aprobado" o "Rechazado"
        
        public string Comentarios { get; set; } = string.Empty;
    }

    public class EditarVersionViewModel
    {
        [Required]
        public int VersionActualId { get; set; }
        
        [Required(ErrorMessage = "Debe subir el nuevo archivo físico.")]
        public IFormFile? ArchivoNuevo { get; set; }
    }
}