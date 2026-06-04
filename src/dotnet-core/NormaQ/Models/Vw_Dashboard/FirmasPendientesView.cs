namespace NormaQ.Models;

public class FirmasPendientesView
{
    public int      DepartamentoId    { get; set; }
    public int      UsuarioId         { get; set; }
    public string   Usuario           { get; set; } = string.Empty;
    public string   Rol               { get; set; } = string.Empty;
    public string   TipoFirma         { get; set; } = string.Empty;
    public byte     Orden             { get; set; }
    public string   DocumentoCodigo   { get; set; } = string.Empty;
    public string   Documento         { get; set; } = string.Empty;
    public string   Version           { get; set; } = string.Empty;
    public DateTime VersionCreadaEn   { get; set; }
}