namespace NormaQ.Models;

public class DocumentosMasVersionesView
{
    public int    DepartamentoId       { get; set; }
    public int    DocumentoId          { get; set; }
    public string Codigo               { get; set; } = string.Empty;
    public string Documento            { get; set; } = string.Empty;
    public string Nivel                { get; set; } = string.Empty;
    public int    TotalVersiones       { get; set; }
    public int    VersionesAprobadas   { get; set; }
    public int    VersionesObsoletas   { get; set; }
    public int    VersionesConRechazo  { get; set; }
}