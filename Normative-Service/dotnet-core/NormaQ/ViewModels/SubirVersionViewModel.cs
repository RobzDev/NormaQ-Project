using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace NormaQ.ViewModels
{
    public class SubirVersionViewModel
    {
        [Required]
        public int DocumentoId { get; set; }

        public string CodigoDocumento { get; set; } = string.Empty;
        public string NombreDocumento { get; set; } = string.Empty;
        public string DepartamentoNombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debe seleccionar un archivo físico.")]
        public IFormFile? ArchivoFisico { get; set; }
    }
}