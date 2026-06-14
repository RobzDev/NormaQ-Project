namespace NormaQ.Models;

public class DocumentApprovedMessage
{
    public string Tipo { get; set; } = "documento_aprobado";
    public int VersionId { get; set; }
}