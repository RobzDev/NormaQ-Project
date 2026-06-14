using System;

namespace NormaQ.Models
{
    public class SolicitudesRegistro
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public int DepartamentoId { get; set; }
        public int RolId { get; set; }
        public string Estado { get; set; } = "Pendiente";
        public DateTime FechaSolicitud { get; set; } = DateTime.UtcNow;

        // Propiedades de Navegación (Mapeo Maestro-Detalle)
        public Departamento? Departamento { get; set; }
        public Role? Rol { get; set; }
    }
}