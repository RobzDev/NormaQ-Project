namespace NormaQ.Models;



public class DocumentoAprobadoView
{
    public int VersionId { get; set; }

    public int DocumentoId { get; set; }

    public string CodigoDocumento { get; set; }

    public string NombreDocumento { get; set; }

    public string Nivel { get; set; }

    public string NormaCodigo { get; set; }

    public string NormaNombre { get; set; }

    public string Departamento { get; set; }

    public string Version { get; set; }

    public string Owner { get; set; }
    public string Estado { get; set; } = string.Empty;

    public string? ApprovedBy { get; set; }

    public DateTime? ApprovedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public string? StoragePath { get; set; }
}