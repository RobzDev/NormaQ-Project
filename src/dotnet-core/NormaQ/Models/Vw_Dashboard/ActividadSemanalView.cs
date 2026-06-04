namespace NormaQ.Models;

public class ActividadSemanalView
{
    public int       DepartamentoId      { get; set; }
    public int       Anio                { get; set; }
    public int       Semana              { get; set; }
    public DateTime  InicioSemana        { get; set; }
    public int       VersionesCreadas    { get; set; }
    public int       VersionesAprobadas  { get; set; }
    public int       VersionesRechazadas { get; set; }
}
