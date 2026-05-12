using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NormaQ.Data;
using NormaQ.Models;
using NormaQ.ViewModels;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Storage;

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
                        RequiereMiIntervencion = //v.Estado == "Revision" && //
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


   

        [HttpGet]
        public async Task<IActionResult> CrearDocumento(int departamentoId)
        {
            // 1. Validación Estricta de Seguridad: ¿Es Admin (Rol 1) en este departamento?
            string claimRequerido = $"{departamentoId}:1";
            if (!User.Claims.Any(c => c.Type == "DeptRole" && c.Value == claimRequerido))
            {
                return Forbid(); // Bloqueo a nivel backend
            }

            var depto = await _context.Departamentos.FindAsync(departamentoId);
            if (depto == null) return NotFound();

            // 2. Ejecución: Consultar solo roles que tienen personal en ESTE departamento
            var rolesConPersonal = await _context.UsuariosRoles
                .Where(ur => ur.DepartamentoId == departamentoId)
                .Select(ur => ur.RolId)
                .Distinct()
                .ToListAsync();

            var model = new CrearDocumentoViewModel
            {
                DepartamentoId = departamentoId,
                DepartamentoNombre = depto.Nombre,

                NivelesDisponibles = await _context.NivelesDocumentos
                    .Select(n => new SelectListItem { Value = n.Id.ToString(), Text = $"Nivel {n.Numero} - {n.Nombre}" })
                    .ToListAsync(),

                NormasDisponibles = await _context.Normas
                    .Select(n => new SelectListItem { Value = n.Id.ToString(), Text = $"{n.Codigo} - {n.Nombre}" })
                    .ToListAsync(),

                RolesDisponibles = await _context.Roles
                    // Filtramos roles (2=Aprobador, 3=Revisor, 4=Elaborador) y exigimos que tengan personal
                    .Where(r => rolesConPersonal.Contains(r.Id) && new[] { 2, 3, 4 }.Contains(r.Id))
                    .Select(r => new SelectListItem { Value = r.Id.ToString(), Text = r.Nombre })
                    .ToListAsync()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearDocumento(CrearDocumentoViewModel model)
        {
            // 1. Re-validación de Seguridad (Evita ataques POST directos)
            if (!User.Claims.Any(c => c.Type == "DeptRole" && c.Value == $"{model.DepartamentoId}:1")) return Forbid();

            if (!ModelState.IsValid)
            {
                // Si falla la validación, recargaríamos los catálogos aquí antes de retornar la vista
                return View(model);
            }

            // ==========================================
            // EJECUCIÓN: INICIO DE TRANSACCIÓN ESTRICTA
            // ==========================================
            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // 2. Generación Inteligente del Código de Documento
                var depto = await _context.Departamentos.FindAsync(model.DepartamentoId);
                var nivel = await _context.NivelesDocumentos.FindAsync(model.NivelId);

                // Mapeo de prefijos
                string prefijoNivel = nivel!.Numero switch { 1 => "MC", 2 => "PR", 3 => "IT", 4 => "RG", _ => "DOC" };

                // Siglas del departamento (Primeras 3 letras mayúsculas)
                string siglasDepto = depto!.Nombre.Length >= 3 ? depto.Nombre.Substring(0, 3).ToUpper() : depto.Nombre.ToUpper();

                // Cálculo del Secuencial
                int conteoExistentes = await _context.Documentos
                    .CountAsync(d => d.DepartamentoId == model.DepartamentoId && d.NivelId == model.NivelId);
                string secuencial = (conteoExistentes + 1).ToString("D2"); // Formato 01, 02, etc.

                string codigoGenerado = $"{prefijoNivel}-{siglasDepto}-{secuencial}";

                int usuarioCreadorId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

                // 3. Inserción Capa 1: Documento Maestro
                var nuevoDocumento = new Documento
                {
                    Codigo = codigoGenerado,
                    Nombre = model.Nombre,
                    NivelId = model.NivelId,
                    NormaId = model.NormaId,
                    DepartamentoId = model.DepartamentoId,
                    CreadoPor = usuarioCreadorId
                   
                };

                _context.Documentos.Add(nuevoDocumento);
                await _context.SaveChangesAsync(); // Se guarda para generar el Identity ID (nuevoDocumento.Id)

                // 4. Inserción Capa 2: Plantilla de Firmas (Respetando UQ_SecFirma_Orden)
                var firmas = new List<SecuenciaFirma>
        {
            new SecuenciaFirma { DocumentoId = nuevoDocumento.Id, RolId = model.RolElaboroId, TipoFirma = "Elaboró", Orden = 1 },
            new SecuenciaFirma { DocumentoId = nuevoDocumento.Id, RolId = model.RolRevisoId, TipoFirma = "Revisó", Orden = 2 },
            new SecuenciaFirma { DocumentoId = nuevoDocumento.Id, RolId = model.RolAproboId, TipoFirma = "Aprobó", Orden = 3 }
        };

                _context.SecuenciaFirmas.AddRange(firmas);
                await _context.SaveChangesAsync();

                // 5. Commit de Transacción
                await transaction.CommitAsync();

                // Redirección exitosa al Dashboard
                return RedirectToAction(nameof(Index), new { selectedDeptId = model.DepartamentoId });
            }
            catch (System.Exception)
            {
                // 6. Rollback Total en caso de cualquier violación de Constraint (ej. Unique Index del Código)
                await transaction.RollbackAsync();
                ModelState.AddModelError(string.Empty, "Ocurrió un error crítico al generar el documento. Se revirtieron los cambios.");
                return View(model);
            }
        }




        [HttpGet]
        public async Task<IActionResult> SubirVersion(int documentoId)
        {
            // 1. Obtener el documento con su información básica y depto
            var documento = await _context.Documentos
                .Include(d => d.Nivel)
                .Include(d => d.Departamento)
                .FirstOrDefaultAsync(d => d.Id == documentoId);

            if (documento == null) return NotFound();

            // 2. Validación de seguridad: ¿El usuario tiene permiso en el depto del documento?
            // Buscamos si existe un claim DeptRole que coincida con el depto del documento y rol Elaborador (Id 4)
            string claimRequerido = $"{documento.DepartamentoId}:4";
            bool esElaboradorAutorizado = User.Claims.Any(c => c.Type == "DeptRole" && c.Value == claimRequerido);

            // También permitimos al Admin (Id 1) por si necesita asistir
            bool esAdminAutorizado = User.Claims.Any(c => c.Type == "DeptRole" && c.Value == $"{documento.DepartamentoId}:1");

            if (!esElaboradorAutorizado && !esAdminAutorizado)
            {
                return Forbid();
            }

            // 3. Pasar el objeto a la vista (usaremos el modelo de la base de datos directamente para esta prueba rápida)
            return View(documento);
        }




    }
}