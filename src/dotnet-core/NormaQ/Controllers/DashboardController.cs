using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace NormaQ.Controllers
{
    [Authorize] // Bloquea el acceso a usuarios no autenticados
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            // No necesitamos consultar la DB aquí todavía.
            // Toda la información ya vive en el objeto 'User' (ClaimsPrincipal) 
            // que el middleware extrajo de la cookie.
            return View();
        }
    }
}