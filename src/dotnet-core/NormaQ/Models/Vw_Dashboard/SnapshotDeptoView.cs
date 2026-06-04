namespace NormaQ.Models;

public class SnapshotDeptoView
{
    public int    DepartamentoId   { get; set; }
    public string Departamento     { get; set; } = string.Empty;
    public int    Borradores       { get; set; }
    public int    EnRevision       { get; set; }
    public int    Aprobados        { get; set; }
    public int    Obsoletos        { get; set; }
    public int    TotalVersiones   { get; set; }
}