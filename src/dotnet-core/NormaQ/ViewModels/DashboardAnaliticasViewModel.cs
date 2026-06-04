using NormaQ.Models;

namespace NormaQ.ViewModels;

public class DashboardAnaliticasViewModel : LayoutViewModel
{

    // Datos de las vistas
    public List<ActividadSemanalView>       ActividadSemanal       { get; set; } = [];
    public List<DocumentosMasVersionesView> DocumentosMasVersiones { get; set; } = [];
    public List<FirmasPendientesView>       FirmasPendientes       { get; set; } = [];
    public List<UsuariosActivosView>        UsuariosActivos        { get; set; } = [];
}