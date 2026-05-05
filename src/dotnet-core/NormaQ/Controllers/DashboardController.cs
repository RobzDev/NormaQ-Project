using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NormaQ.Data;
using NormaQ.ViewModels;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace NormaQ.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly AppDbContext _context;

        public DashboardController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int? selectedDeptId)
        {
            // 1. Ejecución: Extracción de Identidad Base
            var userName = User.FindFirstValue(ClaimTypes.Name);
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var deptoBaseIdStr = User.FindFirstValue("DepartamentoBaseId");
            int deptoBaseId = string.IsNullOrEmpty(deptoBaseIdStr) ? 0 : int.Parse(deptoBaseIdStr);

            int activeDeptId = selectedDeptId ?? deptoBaseId;

            // 2. Ejecución: Resolución de la Malla de Permisos (DeptRole Claims)
            var claimsDeptRole = User.Claims.Where(c => c.Type == "DeptRole").ToList();
            
            var deptIds = claimsDeptRole.Select(c => int.Parse(c.Value.Split(':')[0])).Distinct().ToList();
            var rolIds = claimsDeptRole.Select(c => int.Parse(c.Value.Split(':')[1])).Distinct().ToList();

            var deptosDb = await _context.Departamentos.Where(d => deptIds.Contains(d.Id)).ToListAsync();
            var rolesDb = await _context.Roles.Where(r => rolIds.Contains(r.Id)).ToListAsync();

            var contextosDisponibles = new List<ContextoUsuario>();
            int activeRoleId = 0;

            foreach (var claim in claimsDeptRole)
            {
                var parts = claim.Value.Split(':');
                int dId = int.Parse(parts[0]);
                int rId = int.Parse(parts[1]);

                var depto = deptosDb.FirstOrDefault(d => d.Id == dId);
                var rol = rolesDb.FirstOrDefault(r => r.Id == rId);

                if (depto != null && rol != null && rId != 5) // Omitimos Operario
                {
                    contextosDisponibles.Add(new ContextoUsuario
                    {
                        DepartamentoId = dId,
                        NombreDepartamento = depto.Nombre,
                        NombreRol = rol.Nombre
                    });

                    if (dId == activeDeptId)
                    {
                        activeRoleId = rId;
                    }
                }
            }

            // Validación de seguridad de contexto
            if (activeRoleId == 0 || !contextosDisponibles.Any(c => c.DepartamentoId == activeDeptId))
            {
                return Forbid(); 
            }

            var contextoActivo = contextosDisponibles.First(c => c.DepartamentoId == activeDeptId);

            // 3. Ejecución: Extracción Jerárquica Optimizado (Eager Loading)
            var documentosDelDepto = await _context.Documentos
                .Include(d => d.VersionesDocumentos)
                    .ThenInclude(v => v.FlujosAprobacions) 
                .Where(d => d.DepartamentoId == activeDeptId)
                .ToListAsync();

            var nivelesCatalog = await _context.NivelesDocumentos
                .OrderBy(n => n.Numero)
                .ToListAsync();

            // 4. Ejecución: Construcción del Árbol DTO
            var arbol = nivelesCatalog.Select(nivel => new NivelExploradorDto
            {
                Id = nivel.Id,
                Numero = nivel.Numero,
                Nombre = nivel.Nombre,
                DocumentosLogicos = documentosDelDepto.Where(d => d.NivelId == nivel.Id).Select(doc => new DocumentoExploradorDto
                {
                    Id = doc.Id,
                    Codigo = doc.Codigo,
                    Nombre = doc.Nombre,
                    VersionesFisicas = doc.VersionesDocumentos.Select(v => new VersionExploradorDto
                    {
                        Id = v.Id,
                        VersionMayor = v.VersionMayor,
                        VersionMenor = v.VersionMenor,
                        Estado = v.Estado,
                        RequiereMiIntervencion = v.Estado == "Revision" && 
                                                 v.FlujosAprobacions.Any(f => f.UsuarioId == userId && f.EstadoFirma == "Pendiente")
                    })
                    .OrderByDescending(v => v.VersionMayor)
                    .ThenByDescending(v => v.VersionMenor)
                    .ToList()
                }).ToList()
            }).ToList();

            // 5. Ensamblaje Final
            var vm = new DashboardViewModel
            {
                UsuarioNombre = userName,
                DepartamentoActivoId = activeDeptId,
                DepartamentoActivoNombre = contextoActivo.NombreDepartamento,
                RolActivoNombre = contextoActivo.NombreRol, // El rol ahora es dinámico y real
                ContextosDisponibles = contextosDisponibles,
                ArbolDocumental = arbol
            };

            return View(vm);
        }
    }
}