namespace NormaQ.Models;

public class UsuariosActivosView
{
    public int       DepartamentoId   { get; set; }
    public int       UsuarioId        { get; set; }
    public string    Usuario          { get; set; } = string.Empty;
    public string    Rol              { get; set; } = string.Empty;
    public int       TotalFirmas      { get; set; }
    public int       FirmasAprobadas  { get; set; }
    public int       FirmasRechazadas { get; set; }
    public DateTime? UltimaFirma      { get; set; }
}