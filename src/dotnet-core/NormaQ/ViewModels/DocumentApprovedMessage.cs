namespace NormaQ.ViewModels;

public class DocumentApprovedMessage
{
    public string DocId { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string StoragePath { get; set; } = string.Empty;
    public string CodigoDocumento { get; set; } = string.Empty;
    public string NivelDocumento { get; set; } = string.Empty;
    public string Norma { get; set; } = string.Empty;
    public string Departamento { get; set; } = string.Empty;
    public string Owner { get; set; } = string.Empty;
    public string ApprovedBy { get; set; } = string.Empty;
    public DateTime ApprovedAt { get; set; }
}