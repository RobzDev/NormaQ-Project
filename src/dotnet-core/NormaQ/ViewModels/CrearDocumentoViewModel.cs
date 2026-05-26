using Microsoft.AspNetCore.Mvc.Rendering;

using System.Collections.Generic;

using System.ComponentModel.DataAnnotations;



namespace NormaQ.ViewModels

{

    // ViewModels/CrearDocumentoViewModel.cs

    public class CrearDocumentoViewModel
    {
        // --- Paso 1: Datos del Documento ---
        [Required]
        public int DepartamentoId { get; set; }
        public string DepartamentoNombre { get; set; } = string.Empty;

        [Required, MaxLength(255)]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        public int NivelId { get; set; }

        [Required]
        public int NormaId { get; set; }

        public List<SelectListItem> NivelesDisponibles { get; set; } = new();
        public List<SelectListItem> NormasDisponibles { get; set; } = new();

        // --- Paso 2: Secuencia de Firmas ---
        // El admin elige cuántos slots de cada tipo quiere,
        // pero el frontend ya pre-rellena 1 de cada uno.
        // Cada lista contiene los RolId seleccionados en orden.
        // Como todos los slots son del mismo rol, RolId es constante;
        // lo que varía es el orden global en la secuencia.

        [Required, MinLength(1)]
        public List<int> ElaboradoresSeleccionados { get; set; } = new() { 4 }; // mínimo 1

        [Required, MinLength(1)]
        public List<int> RevisoresSeleccionados { get; set; } = new() { 3 };   // mínimo 1

        [Required, MinLength(1)]
        public List<int> AprobadoresSeleccionados { get; set; } = new() { 2 }; // mínimo 1

        // --- Disponibilidad para el frontend (solo lectura, no se postean) ---
        public int TotalElaboradoresDisponibles { get; set; }
        public int TotalRevisoresDisponibles { get; set; }
        public int TotalAprobadoresDisponibles { get; set; }
    }

}

