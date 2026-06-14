using System;
using System.Collections.Generic;
using NormaQ.Models;

namespace NormaQ.ViewModels
{

    public class LayoutViewModel
    {
        public string UsuarioNombre { get; set; } = string.Empty;
        public int DepartamentoActivoId { get; set; }
        public string DepartamentoActivoNombre { get; set; } = string.Empty;
        public string RolActivoNombre { get; set; } = string.Empty;
    }

    public class DashboardViewModel: LayoutViewModel
    {
        // Selector de Contextos
        public List<ContextoUsuario> ContextosDisponibles { get; set; } = new();

        // Explorador Documental Jerárquico (Niveles -> Documentos -> Versiones)
        public List<NivelExploradorDto> ArbolDocumental { get; set; } = new();
        public List<NotificacionDto> Notificaciones { get; set; } = new();
        public SnapshotDeptoView? Snapshot { get; set; }

        public string CompaniaNombre { get; set; } = string.Empty;

    }

    public class ContextoUsuario
    {
        public int DepartamentoId { get; set; }
        public string NombreDepartamento { get; set; } = string.Empty;
        public string NombreRol { get; set; } = string.Empty;
    }

    // CAPA 1: Nivel ISO
    public class NivelExploradorDto
    {
        public int Id { get; set; }
        public byte Numero { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public List<DocumentoExploradorDto> DocumentosLogicos { get; set; } = new();
    }

    // CAPA 2: Documento Lógico (Identidad inmutable)
    public class DocumentoExploradorDto
    {
        public int Id { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public List<VersionExploradorDto> VersionesFisicas { get; set; } = new();
    }

    // CAPA 3: Versión Física (Ciclo de vida)
    public class VersionExploradorDto
    {
        public int Id { get; set; }
        public byte VersionMayor { get; set; }
        public byte VersionMenor { get; set; }
        public string Estado { get; set; } = string.Empty;

        // Fecha en que se subió/creó la versión
        public DateTime FechaSubida { get; set; }

        // Nombre del usuario creador (para mostrar en UI)
        public string CreadoPor { get; set; } = string.Empty;
        
        // Bandera inteligente UI/UX
        public bool RequiereMiIntervencion { get; set; } 

        public bool EstaRechazado { get; set; }
    }

    public class NotificacionDto
    {
        public string DocumentoCodigo { get; set; } = string.Empty;
        public string VersionLabel { get; set; } = string.Empty;
        public string NivelNombre { get; set; } = string.Empty;
        public int VersionId { get; set; }
    }
}