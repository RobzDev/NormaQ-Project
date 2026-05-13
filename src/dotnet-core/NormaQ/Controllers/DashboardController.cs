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
using System.IO;
using NormaQ.Services;

namespace NormaQ.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly AppDbContext _context;
        private readonly MinioService _minioService;

        public DashboardController(AppDbContext context, MinioService minioService)
        {
            _context = context;
            _minioService = minioService;
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
                        RequiereMiIntervencion = v.FlujosAprobacions
                            .Any(f =>
                            f.UsuarioId == userId &&
                            f.EstadoFirma == "Pendiente" &&
                            v.FlujosAprobacions
                            .Where(prev => prev.Orden < f.Orden)
                            .All(prev => prev.EstadoFirma == "Aprobado")
                       )
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
            var documento = await _context.Documentos
                .Include(d => d.Departamento)
                .FirstOrDefaultAsync(d => d.Id == documentoId);

            if (documento == null) return NotFound();

            // Validación de seguridad (Elaborador o Admin)
            bool autorizado = User.Claims.Any(c => c.Type == "DeptRole" &&
                             (c.Value == $"{documento.DepartamentoId}:4" || c.Value == $"{documento.DepartamentoId}:1"));

            if (!autorizado) return Forbid();

            var model = new SubirVersionViewModel
            {
                DocumentoId = documento.Id,
                CodigoDocumento = documento.Codigo,
                NombreDocumento = documento.Nombre,
                DepartamentoNombre = documento.Departamento.Nombre
            };

            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubirVersion(SubirVersionViewModel model)
        {
            if (!ModelState.IsValid || model.ArchivoFisico == null) return View(model);

            // 1. Ejecución: Recuperar contexto del documento
            var documento = await _context.Documentos
                .Include(d => d.Nivel) // Corregido a PascalCase según tu AppDbContext
                .Include(d => d.VersionesDocumentos)
                .FirstOrDefaultAsync(d => d.Id == model.DocumentoId);

            if (documento == null) return NotFound();

            // 2. Ejecución: Lógica de Versionamiento (A, B, C)
            byte vMayor = 1, vMenor = 0;
            var ultimaVersion = documento.VersionesDocumentos
                .OrderByDescending(v => v.VersionMayor).ThenByDescending(v => v.VersionMenor)
                .FirstOrDefault();

            if (ultimaVersion != null)
            {
                if (ultimaVersion.Estado == "Aprobado") { vMayor = (byte)(ultimaVersion.VersionMayor + 1); vMenor = 0; }
                else { vMayor = ultimaVersion.VersionMayor; vMenor = (byte)(ultimaVersion.VersionMenor + 1); }
            }

            // 3. Preparar Identificador para MinIO
            string extension = Path.GetExtension(model.ArchivoFisico.FileName).ToLower();
            string rutaMinio = $"{documento.DepartamentoId}/Nivel_{documento.Nivel.Numero}/{documento.Codigo}-v{vMayor}.{vMenor}{extension}";

            // ==========================================
            // INICIO DE OPERACIÓN ATÓMICA (MinIO + SQL)
            // ==========================================

            string minioId = string.Empty;
            try
            {
                // A. Subir archivo físico vía AWSSDK.S3
                minioId = await _minioService.SubirArchivoAsync(model.ArchivoFisico, rutaMinio);

                // B. Iniciar Transacción en Base de Datos
                await using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    var nuevaVersion = new VersionesDocumento
                    {
                        DocumentoId = documento.Id,
                        VersionMayor = vMayor,
                        VersionMenor = vMenor,
                        Estado = "Borrador",
                        MinioIdentifier = minioId,
                        CreadoPor = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!),
                        FechaCreacion = DateTime.Now
                    };

                    _context.VersionesDocumentos.Add(nuevaVersion);
                    await _context.SaveChangesAsync();

                    // C. Ejecución del Stored Procedure (Malla de autorización)
                    await _context.Database.ExecuteSqlRawAsync(
                        "EXEC SP_Generar_Flujos_Aprobacion @version_id = {0};",
                        nuevaVersion.Id
                    );

                    await transaction.CommitAsync();

                    // Éxito total: Redirigir al depto donde se subió
                    return RedirectToAction("Index", new { selectedDeptId = documento.DepartamentoId });
                }
                catch (Exception)
                {
                    // Rollback DB
                    await transaction.RollbackAsync();
                    throw; // Re-lanzar para que el catch externo limpie MinIO
                }
            }
            catch (Exception ex)
            {
                // D. Rollback Físico: Eliminar de MinIO si la DB falló para evitar huérfanos
                if (!string.IsNullOrEmpty(minioId))
                {
                    await _minioService.EliminarArchivoAsync(minioId);
                }

                ModelState.AddModelError(string.Empty, "Error crítico: " + ex.Message);
                return View(model);
            }
        }


        [HttpGet]
        public async Task<IActionResult> VisualizarVersion(int versionId, bool intervencion)
        {
            // 1. Ejecución: Obtener la versión con su documento y jerarquía
            var version = await _context.VersionesDocumentos
                .Include(v => v.Documento)
                    .ThenInclude(d => d.Nivel)
                .Include(v => v.Documento)
                    .ThenInclude(d => d.Departamento)
                .Include(v => v.FlujosAprobacions)
                .FirstOrDefaultAsync(v => v.Id == versionId);

            if (version == null) return NotFound();

            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            // 2. Determinar si el usuario tiene una tarea pendiente en esta versión
            // (Lógica similar a la del árbol para mantener consistencia)
           

            // Pasamos la bandera por ViewBag para la vista
            ViewBag.RequiereMiIntervencion = intervencion;

            return View(version);
        }
        [HttpGet]
        public async Task<IActionResult> DescargarArchivo(int versionId, bool inline = false)
        {
            var version = await _context.VersionesDocumentos.FindAsync(versionId);
            if (version == null || string.IsNullOrEmpty(version.MinioIdentifier)) return NotFound();

            Console.WriteLine($"[DEBUG] Valor de miVariable: {version.MinioIdentifier}");

            var responseStream = await _minioService.ObtenerArchivoAsync(version.MinioIdentifier);

            // Detectar MIME type correcto según extensión
            var ext = Path.GetExtension(version.MinioIdentifier).ToLower();
            var mimeType = ext switch
            {
                ".pdf" => "application/pdf",
                ".png" => "image/png",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".gif" => "image/gif",
                ".webp" => "image/webp",
                ".txt" => "text/plain",
                ".md" => "text/plain",
                _ => "application/octet-stream"
            };

            var fileName = Path.GetFileName(version.MinioIdentifier);

            // inline=true → el navegador muestra el archivo (visor)
            // inline=false → fuerza descarga (botón de descarga)
            if (inline)
                Response.Headers["Content-Disposition"] = $"inline; filename=\"{fileName}\"";

            return File(responseStream, mimeType, inline ? null : fileName);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FirmarDocumento(FirmarVersionViewModel model)
        {
            if (model.Accion == "Rechazado" && string.IsNullOrWhiteSpace(model.Comentarios))
            {
                TempData["ErrorFirma"] = "Los comentarios son obligatorios al rechazar un documento.";
                return RedirectToAction("VisualizarVersion", new { versionId = model.VersionId });
            }

            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            // 1. Encontrar la tarea de este usuario
            var flujoActual = await _context.FlujosAprobacions
                .FirstOrDefaultAsync(f => f.VersionId == model.VersionId
                                       && f.UsuarioId == userId
                                       && f.EstadoFirma == "Pendiente");

            if (flujoActual == null) return NotFound("No tienes tareas pendientes para esta versión o ya fue procesada.");

            // INICIO DE TRANSACCIÓN PARA CONCURRENCIA
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 2. Control de Concurrencia masivo: Cancelar a los demás aprobadores del MISMO ORDEN
                // Usamos ExecuteUpdateAsync para un UPDATE directo y rápido en SQL Server
                await _context.FlujosAprobacions
                    .Where(f => f.VersionId == model.VersionId
                             && f.Orden == flujoActual.Orden
                             && f.Id != flujoActual.Id
                             && f.EstadoFirma == "Pendiente")
                    .ExecuteUpdateAsync(s => s.SetProperty(f => f.EstadoFirma, "Cancelado"));

                // 3. Actualizar la firma del usuario actual
                flujoActual.EstadoFirma = model.Accion;
                flujoActual.Comentarios = model.Comentarios ?? string.Empty;
                flujoActual.FechaFirma = DateTime.UtcNow;

                // NOTA: No tocamos Versiones_Documento. El Trigger 'trg_MutarEstadoVersion' 
                // escuchará este SaveChanges y evaluará si muta de Borrador -> Revisión -> Aprobado.
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                return RedirectToAction("VisualizarVersion", new { versionId = model.VersionId });
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        // ==========================================
        // ACCIÓN 3: EDITAR (Subir Nueva Versión Menor)
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarVersion(EditarVersionViewModel model)
        {
            if (!ModelState.IsValid || model.ArchivoNuevo == null)
                return RedirectToAction("VisualizarVersion", new { versionId = model.VersionActualId });

            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var versionActual = await _context.VersionesDocumentos
                .Include(v => v.Documento).ThenInclude(d => d.Nivel)
                .Include(v => v.Documento).ThenInclude(d => d.Departamento)
                .FirstOrDefaultAsync(v => v.Id == model.VersionActualId);

            if (versionActual == null) return NotFound();

            // Nueva versión menor
            byte vMayor = versionActual.VersionMayor;
            byte vMenor = (byte)(versionActual.VersionMenor + 1);

            string extension = Path.GetExtension(model.ArchivoNuevo.FileName).ToLower();
            string rutaMinio = $"{versionActual.Documento.DepartamentoId}/Nivel_{versionActual.Documento.Nivel.Numero}/{versionActual.Documento.Codigo}-v{vMayor}.{vMenor}{extension}";

            string minioId = string.Empty;
            try
            {
                // A. Subir a MinIO
                minioId = await _minioService.SubirArchivoAsync(model.ArchivoNuevo, rutaMinio);

                await using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    // B. Insertar BD
                    var nuevaVersion = new VersionesDocumento
                    {
                        DocumentoId = versionActual.DocumentoId,
                        VersionMayor = vMayor,
                        VersionMenor = vMenor,
                        Estado = "Borrador",
                        MinioIdentifier = minioId,
                        CreadoPor = userId,
                        FechaCreacion = DateTime.UtcNow
                    };

                    _context.VersionesDocumentos.Add(nuevaVersion);
                    await _context.SaveChangesAsync();

                    // C. Ejecutar SP
                    await _context.Database.ExecuteSqlRawAsync("EXEC SP_Generar_Flujos_Aprobacion @version_id = {0};", nuevaVersion.Id);

                    await transaction.CommitAsync();

                    // Redirigir a la vista de la NUEVA versión
                    return RedirectToAction("VisualizarVersion", new { versionId = nuevaVersion.Id });
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                if (!string.IsNullOrEmpty(minioId)) await _minioService.EliminarArchivoAsync(minioId);
                TempData["ErrorSistema"] = "Fallo la transacción: " + ex.Message;
                return RedirectToAction("VisualizarVersion", new { versionId = model.VersionActualId });
            }
        }




    }
}