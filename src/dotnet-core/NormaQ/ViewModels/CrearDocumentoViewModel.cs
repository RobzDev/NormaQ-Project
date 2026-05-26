using Microsoft.AspNetCore.Mvc.Rendering;

using System.Collections.Generic;

using System.ComponentModel.DataAnnotations;



namespace NormaQ.ViewModels

{

    public class CrearDocumentoViewModel

    {

        [Required]

        public int DepartamentoId { get; set; }

        public string DepartamentoNombre { get; set; } = string.Empty;



        // ==========================================

        // PASO 1: Identidad Lógica

        // ==========================================

        [Required(ErrorMessage = "El nombre del documento es obligatorio")]

        [StringLength(255)]

        [Display(Name = "Nombre del Documento")]

        public string Nombre { get; set; } = string.Empty;



        [Required(ErrorMessage = "Debe seleccionar un Nivel")]

        [Display(Name = "Nivel ISO")]

        public int NivelId { get; set; }



        [Required(ErrorMessage = "Debe seleccionar una Norma")]

        [Display(Name = "Norma Aplicable")]

        public int NormaId { get; set; }



        // ==========================================

        // PASO 2: Plantilla de Firmas

        // ==========================================

        [Required(ErrorMessage = "Debe asignar un rol para Elaborar")]

        [Display(Name = "Rol que Elabora")]

        public int RolElaboroId { get; set; }



        [Required(ErrorMessage = "Debe asignar un rol para Revisar")]

        [Display(Name = "Rol que Revisa")]

        public int RolRevisoId { get; set; }



        [Required(ErrorMessage = "Debe asignar un rol para Aprobar")]

        [Display(Name = "Rol que Aprueba")]

        public int RolAproboId { get; set; }



        // ==========================================

        // CATÁLOGOS (Listas para los <select>)

        // ==========================================

        public List<SelectListItem> NivelesDisponibles { get; set; } = new();

        public List<SelectListItem> NormasDisponibles { get; set; } = new();



        // Esta lista será inteligente: solo roles con usuarios en el departamento actual

        public List<SelectListItem> RolesDisponibles { get; set; } = new();

    }

}

