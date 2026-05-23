using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NormaQ.ViewModels
{
    // ViewModel para la vista GET (Lista de solicitudes del departamento)
    public class GestionarSolicitudesViewModel
    {
        public int DepartamentoId { get; set; }
        public string NombreDepartamento { get; set; } = string.Empty;
        public List<SolicitudPendienteDto> Solicitudes { get; set; } = new();
    }

    public class SolicitudPendienteDto
    {
        public int SolicitudId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string NombreRolSugerido { get; set; } = string.Empty;
        public DateTime FechaSolicitud { get; set; }
    }

    // ViewModel para el POST (Cuando el admin da clic en Aprobar o Rechazar)
    public class ProcesarSolicitudViewModel
    {
        [Required]
        public int SolicitudId { get; set; }

        [Required]
        public string Accion { get; set; } = string.Empty; // "Aprobar" o "Rechazar"
    }
}