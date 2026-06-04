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
        private readonly RedisPublisherService _redisPublisher;
        private readonly IEmailService _emailService; // Añadido para notificar al usuario

        public DashboardController(AppDbContext context, MinioService minioService, RedisPublisherService redisPublisher, IEmailService emailService)
        {
            _context = context;
            _minioService = minioService;
            _redisPublisher = redisPublisher;
            _emailService = emailService;
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
                        .ThenInclude(f => f.Usuario)
                .Include(d => d.VersionesDocumentos)
                    .ThenInclude(v => v.CreadoPorNavigation)
                .Where(d => d.DepartamentoId == activeDeptId)
                .ToListAsync();

            var nivelesCatalog = await _context.NivelesDocumentos
                .OrderBy(n => n.Numero)
                .ToListAsync();


            var notificaciones = new List<NotificacionDto>();
            

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
                    VersionesFisicas = doc.VersionesDocumentos.Select(v =>
                    {
                        bool requiere = v.FlujosAprobacions
                        .Any(f =>
                            f.UsuarioId == userId &&
                            f.EstadoFirma == "Pendiente" &&
                            // Cambiamos el .All de aprobación estricta por un .Any de inexistencia de pendientes previos
                            !v.FlujosAprobacions
                                .Any(prev => prev.Orden < f.Orden && prev.EstadoFirma == "Pendiente")
                        );
                        


                        if (requiere)
                            notificaciones.Add(new NotificacionDto
                            {
                                DocumentoCodigo = doc.Codigo,
                                VersionLabel = $"v{v.VersionMayor}.{v.VersionMenor}",
                                NivelNombre = nivel.Nombre,
                                VersionId = v.Id
                            });

                        bool estaRechazado = v.FlujosAprobacions.Any(f => f.EstadoFirma == "Rechazado");   
                        Console.WriteLine($"[DEBUG] Versión {v.Id} - RequiereIntervención: {requiere}, EstáRechazado: {estaRechazado}"); 

                        return new VersionExploradorDto
                        {
                            Id = v.Id,
                            VersionMayor = v.VersionMayor,
                            VersionMenor = v.VersionMenor,
                            Estado = v.Estado,
                            RequiereMiIntervencion = requiere,
                            EstaRechazado = estaRechazado,
                            FechaSubida = v.FechaCreacion,
                            CreadoPor = v.CreadoPorNavigation != null ? v.CreadoPorNavigation.Nombre : v.CreadoPor.ToString()
                        };
                        
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
                Notificaciones = notificaciones, // Pasamos las notificaciones a la vista
                ArbolDocumental = arbol,
                Snapshot = await _context.SnapshotDepto
                .Where(x => x.DepartamentoId == activeDeptId)
                .FirstOrDefaultAsync()

            };


            return View(vm);
        }
        [HttpGet]
        public async Task<IActionResult> CrearDocumento(int departamentoId)
        {
            var deptRoles = User.Claims
                .Where(c => c.Type == "DeptRole")
                .Select(c => c.Value)
                .ToList();
            
            Console.WriteLine($"[DEBUG] === DeptRoles extraídos: {deptRoles.Count}");
            foreach (var rol in deptRoles)
            {
                Console.WriteLine($"[DEBUG]   - Rol: {rol}");
            }
            Console.WriteLine($"[DEBUG] === Buscando claim: {departamentoId}:1");




            // 1. Seguridad: solo Admin (Rol 1) de este departamento
            string claimEsperado = $"{departamentoId}:1";
            bool tienePermisoAdminDepto = deptRoles.Contains(claimEsperado);
            if (!tienePermisoAdminDepto)
            {
                string claimsActuales = deptRoles.Count > 0
                    ? string.Join(", ", deptRoles)
                    : "(sin claims DeptRole)";

                return RedirectToAction(
                    "AccessDenied",
                    "Account",
                    new
                    {
                        reason = "FaltaPermisoAdminDepto",
                        expected = claimEsperado,
                        actual = claimsActuales
                    }
                );
            }

            var depto = await _context.Departamentos.FindAsync(departamentoId);
            if (depto == null) return NotFound();

            // 2. Contar usuarios disponibles por rol en este departamento
            //    (roles 2=Aprobador, 3=Revisor, 4=Elaborador)
            var conteosPorRol = await _context.UsuariosRoles
                .Where(ur => ur.DepartamentoId == departamentoId
                          && new[] { 2, 3, 4 }.Contains(ur.RolId))
                .GroupBy(ur => ur.RolId)
                .Select(g => new { RolId = g.Key, Total = g.Count() })
                .ToListAsync();

            int Contar(int rolId) => conteosPorRol.FirstOrDefault(x => x.RolId == rolId)?.Total ?? 0;

            // 3. Validación temprana: si falta personal en algún rol crítico,
            //    no tiene sentido abrir el wizard.
            int totalElaboradores = Contar(4);
            int totalRevisores = Contar(3);
            int totalAprobadores = Contar(2);

            if (totalElaboradores == 0 || totalRevisores == 0 || totalAprobadores == 0)
            {
                TempData["Error"] = "El departamento no tiene personal suficiente en todos los roles requeridos " +
                                    "(Elaborador, Revisor, Aprobador). Asigna usuarios antes de crear un documento.";
                return RedirectToAction(nameof(Index), new { selectedDeptId = departamentoId });
            }

            var model = new CrearDocumentoViewModel
            {
                DepartamentoId = departamentoId,
                DepartamentoNombre = depto.Nombre,

                TotalElaboradoresDisponibles = totalElaboradores,
                TotalRevisoresDisponibles = totalRevisores,
                TotalAprobadoresDisponibles = totalAprobadores,

                // Listas pre-rellenadas con 1 slot cada una (valor = RolId)
                ElaboradoresSeleccionados = new List<int> { 4 },
                RevisoresSeleccionados = new List<int> { 3 },
                AprobadoresSeleccionados = new List<int> { 2 },

                NivelesDisponibles = await _context.NivelesDocumentos
                    .Select(n => new SelectListItem
                    {
                        Value = n.Id.ToString(),
                        Text = $"Nivel {n.Numero} - {n.Nombre}"
                    })
                    .ToListAsync(),

                NormasDisponibles = await _context.Normas
                    .Select(n => new SelectListItem
                    {
                        Value = n.Id.ToString(),
                        Text = $"{n.Codigo} - {n.Nombre}"
                    })
                    .ToListAsync()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearDocumento(CrearDocumentoViewModel model)
        {
            // 1. Re-validación de seguridad (bloquea ataques POST directos)
            if (!User.Claims.Any(c => c.Type == "DeptRole" && c.Value == $"{model.DepartamentoId}:1"))
                return Forbid();

            // 2. Validaciones de negocio sobre la secuencia de firmas
            //    antes de tocar la BD para dar feedback limpio al usuario.
            var conteosPorRol = await _context.UsuariosRoles
                .Where(ur => ur.DepartamentoId == model.DepartamentoId
                          && new[] { 2, 3, 4 }.Contains(ur.RolId))
                .GroupBy(ur => ur.RolId)
                .Select(g => new { RolId = g.Key, Total = g.Count() })
                .ToListAsync();

            int Contar(int rolId) => conteosPorRol.FirstOrDefault(x => x.RolId == rolId)?.Total ?? 0;

            model.TotalElaboradoresDisponibles = Contar(4);
            model.TotalRevisoresDisponibles = Contar(3);
            model.TotalAprobadoresDisponibles = Contar(2);

            // Slots solicitados vs disponibles
            if (model.ElaboradoresSeleccionados.Count > model.TotalElaboradoresDisponibles)
                ModelState.AddModelError(nameof(model.ElaboradoresSeleccionados),
                    $"Solo hay {model.TotalElaboradoresDisponibles} elaborador(es) disponible(s) en este departamento.");

            if (model.RevisoresSeleccionados.Count > model.TotalRevisoresDisponibles)
                ModelState.AddModelError(nameof(model.RevisoresSeleccionados),
                    $"Solo hay {model.TotalRevisoresDisponibles} revisor(es) disponible(s) en este departamento.");

            if (model.AprobadoresSeleccionados.Count > model.TotalAprobadoresDisponibles)
                ModelState.AddModelError(nameof(model.AprobadoresSeleccionados),
                    $"Solo hay {model.TotalAprobadoresDisponibles} aprobador(es) disponible(s) en este departamento.");

            if (!ModelState.IsValid)
            {
                // Recargar catálogos antes de devolver la vista
                model.NivelesDisponibles = await _context.NivelesDocumentos
                    .Select(n => new SelectListItem
                    {
                        Value = n.Id.ToString(),
                        Text = $"Nivel {n.Numero} - {n.Nombre}"
                    })
                    .ToListAsync();

                model.NormasDisponibles = await _context.Normas
                    .Select(n => new SelectListItem
                    {
                        Value = n.Id.ToString(),
                        Text = $"{n.Codigo} - {n.Nombre}"
                    })
                    .ToListAsync();

                return View(model);
            }

            // 3. Construir la secuencia de firmas con orden global estricto:
            //    Elaboradores (1..N) → Revisores (N+1..M) → Aprobadores (M+1..Z)
            var firmas = new List<SecuenciaFirma>();
            byte orden = 1;

            foreach (int rolId in model.ElaboradoresSeleccionados)
                firmas.Add(new SecuenciaFirma { RolId = rolId, TipoFirma = "Elaboró", Orden = orden++ });

            foreach (int rolId in model.RevisoresSeleccionados)
                firmas.Add(new SecuenciaFirma { RolId = rolId, TipoFirma = "Revisó", Orden = orden++ });

            foreach (int rolId in model.AprobadoresSeleccionados)
                firmas.Add(new SecuenciaFirma { RolId = rolId, TipoFirma = "Aprobó", Orden = orden++ });

            // 4. Transacción estricta
            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // 4a. Generar código del documento
                var depto = await _context.Departamentos.FindAsync(model.DepartamentoId);
                var nivel = await _context.NivelesDocumentos.FindAsync(model.NivelId);

                string prefijoNivel = nivel!.Numero switch
                {
                    1 => "MC",
                    2 => "PR",
                    3 => "IT",
                    4 => "RG",
                    _ => "DOC"
                };

                string siglasDepto = depto!.Nombre.Length >= 3
                    ? depto.Nombre[..3].ToUpper()
                    : depto.Nombre.ToUpper();

                int conteoExistentes = await _context.Documentos
                    .CountAsync(d => d.DepartamentoId == model.DepartamentoId
                                  && d.NivelId == model.NivelId);

                string codigoGenerado = $"{prefijoNivel}-{siglasDepto}-{(conteoExistentes + 1):D2}";

                int usuarioCreadorId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

                // 4b. Insertar Documento maestro
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
                await _context.SaveChangesAsync(); // Genera el Id del documento

                // 4c. Vincular firmas al documento recién creado
                foreach (var firma in firmas)
                    firma.DocumentoId = nuevoDocumento.Id;

                _context.SecuenciaFirmas.AddRange(firmas);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return RedirectToAction(nameof(Index), new { selectedDeptId = model.DepartamentoId });
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                ModelState.AddModelError(string.Empty,
                    "Ocurrió un error al guardar el documento. Es posible que el código generado ya exista. Se revirtieron los cambios.");

                // Recargar catálogos
                model.NivelesDisponibles = await _context.NivelesDocumentos
                    .Select(n => new SelectListItem { Value = n.Id.ToString(), Text = $"Nivel {n.Numero} - {n.Nombre}" })
                    .ToListAsync();

                model.NormasDisponibles = await _context.Normas
                    .Select(n => new SelectListItem { Value = n.Id.ToString(), Text = $"{n.Codigo} - {n.Nombre}" })
                    .ToListAsync();

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

            var minioIdentifier = await _context.VersionesDocumentos
                .AsNoTracking()
                .Where(v => v.DocumentoId == documentoId && !string.IsNullOrEmpty(v.MinioIdentifier))
                .OrderByDescending(v => v.Id)
                .Select(v => v.MinioIdentifier)
                .FirstOrDefaultAsync();


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
                .Include(d => d.Departamento).ThenInclude(dep => dep.Compania)
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
            string rutaMinio = $"{documento.Departamento.Compania.Id}/{documento.DepartamentoId}/Nivel_{documento.Nivel.Numero}/{documento.Codigo}-v{vMayor}.{vMenor}{extension}";
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
        public async Task<IActionResult> VisualizarVersion(int versionId)
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

            // 3. Ejecución: Identificar el paso secuencial activo en la base de datos
            // Buscamos el número de orden más bajo que todavía está "Pendiente" para esta versión.
            // Usamos (int?) para evitar excepciones si ya no quedan firmas pendientes en la lista.
            var ordenActualActivo = version.FlujosAprobacions
                .Where(f => f.EstadoFirma == "Pendiente")
                .Select(f => (int?)f.Orden)
                .Min();

            // 4. Ejecución: Evaluación de la barrera de turno
            // El usuario tiene autorización de edición/firma si y solo si:
            // - La versión está globalmente en estado "Revision".
            // - El usuario tiene un flujo asignado en estado "Pendiente".
            // - El orden de su firma es igual al orden activo más bajo (es su turno real).
            bool intervencion = 
                                ordenActualActivo.HasValue &&
                                version.FlujosAprobacions.Any(f => f.UsuarioId == userId &&
                                                                   f.EstadoFirma == "Pendiente" &&
                                                                   f.Orden == ordenActualActivo.Value);

            // Pasamos la bandera calculada por ViewBag para mantener la compatibilidad con tu vista
            ViewBag.RequiereMiIntervencion = intervencion;

            //ver si el documento es editable por el usuario (si esta en algun flujo de aprobacion y cuya firma no sea "Cancelada" y )
            bool esEditable = version.FlujosAprobacions.Any(f => f.UsuarioId == userId && f.EstadoFirma != "Cancelado");
            ViewBag.Editable = esEditable;

            return View(version);
        }
        [HttpGet]
        public async Task<IActionResult> DescargarArchivo(int versionId, bool inline = false)
        {
            var version = await _context.VersionesDocumentos.FindAsync(versionId);
            if (version == null || string.IsNullOrEmpty(version.MinioIdentifier)) return NotFound();

            

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
            Console.WriteLine($"[DEBUG] Acción recibida: {model.Accion} para versión {model.VersionId} con comentarios: '{model.Comentarios}'");

            if (model.Accion == "Rechazado" && string.IsNullOrWhiteSpace(model.Comentarios))
            {
                TempData["ErrorFirma"] = "Los comentarios son obligatorios al rechazar un documento.";
                return RedirectToAction("VisualizarVersion", new { versionId = model.VersionId });
            }

            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            // 1. Encontrar la tarea de este usuario
            var flujoActual = await _context.FlujosAprobacions
                .Include(f => f.Usuario)
                .FirstOrDefaultAsync(f => f.VersionId == model.VersionId
                                       && f.UsuarioId == userId
                                       && f.EstadoFirma == "Pendiente");

            if (flujoActual == null) return NotFound("No tienes tareas pendientes para esta versión o ya fue procesada.");

            // INICIO DE TRANSACCIÓN PARA CONCURRENCIA
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 2. Control de Concurrencia masivo: Cancelar a los demás aprobadores del MISMO ORDEN
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

                await _context.SaveChangesAsync();
                await transaction.CommitAsync(); // Se consolida el cambio y el Trigger SQL actúa de inmediato

                // ============================================================
                // 4. CONSULTA POST-TRIGGER: Resolución Limpia de Datos
                // ============================================================
                var versionData = await _context.VersionesDocumentos
                .AsNoTracking()
                .Include(v => v.Documento)
                    .ThenInclude(d => d.Departamento)
                .FirstOrDefaultAsync(v => v.Id == model.VersionId);

                if (versionData != null)
                {
                    Console.WriteLine($"[DEBUG] Estado detectado post-trigger: {versionData.Estado}");

                    string linkVersion = Url.Action("VisualizarVersion", "Dashboard", new { versionId = versionData.Id }, Request.Scheme) ?? string.Empty;
                    string docNombreCompleto = $"{versionData.Documento.Nombre} (v{versionData.VersionMayor}.{versionData.VersionMenor})";

                    // ============================================================
                    // CONTEXTO A: DOCUMENTO TOTALMENTE APROBADO
                    // ============================================================
                    if (versionData.Estado == "Aprobado")
                    {
                        await _redisPublisher.PublishDocumentAsync(new DocumentApprovedMessage
                        {
                            VersionId = versionData.Id
                        });
                        Console.WriteLine($"[Redis] Notificación enviada para versión {versionData.Id}");

                        // Extraer correos de todos los que participaron activamente sin ser cancelados
                        var usuariosANotificar = await _context.FlujosAprobacions
                            .Where(f => f.VersionId == versionData.Id && f.EstadoFirma != "Cancelado")
                            .Select(f => f.Usuario.Email)
                            .ToListAsync();

                        // EJECUCIÓN DIRECTA: Resolver el correo del creador usando su ID entero
                        var emailCreadorFinal = await _context.Usuarios
                            .Where(u => u.Id == versionData.CreadoPor)
                            .Select(u => u.Email)
                            .FirstOrDefaultAsync();

                        if (!string.IsNullOrEmpty(emailCreadorFinal))
                        {
                            usuariosANotificar.Add(emailCreadorFinal);
                        }

                        string cuerpoHtmlAprobado = $@"
                        <div style='font-family: Arial, sans-serif; padding: 20px; color: #333;'>
                            <h2 style='color: #10b981;'>QualityDoc — Documento Aprobado Oficialmente</h2>
                            <p>Nos complace informarle que el documento normativo ha concluido exitosamente su ciclo de firmas:</p>
                            <blockquote style='background: #f0fdf4; padding: 15px; border-left: 4px solid #10b981; font-weight: bold;'>
                                {docNombreCompleto}
                            </blockquote>
                            <p>El archivo ya se encuentra disponible para consulta en el repositorio de <strong>{versionData.Documento.Departamento.Nombre}</strong>.</p>                  
                            <br/>
                            <a href='{linkVersion}' style='background-color: #10b981; color: #fff; padding: 12px 24px; text-decoration: none; border-radius: 6px; font-weight: bold; display: inline-block;'>
                                Abrir Documento Aprobado
                            </a>
                            <p style='font-size: 12px; color: #888; margin-top: 40px;'>Sistema de Gestión ISO 9001 - QualityDoc Polyglot</p>
                        </div>";

                        foreach (var email in usuariosANotificar.Distinct())
                        {
                            await _emailService.EnviarCorreoAsync(email, $"Liberación: {docNombreCompleto}", cuerpoHtmlAprobado);
                        }
                    }
                    // ============================================================
                    // CONTEXTO B: SE MANTIENE EN REVISIÓN (AVANZA AL SIGUIENTE ORDEN)
                    // ============================================================
                    else if (versionData.Estado == "Revision" && model.Accion == "Aprobado")
                    {
                        var siguientesAprobadores = await _context.FlujosAprobacions
                            .Include(f => f.Usuario)
                            .Where(f => f.VersionId == versionData.Id
                                     && f.EstadoFirma == "Pendiente"
                                     && f.Orden == flujoActual.Orden + 1)
                            .ToListAsync();

                        foreach (var siguiente in siguientesAprobadores)
                        {
                            string cuerpoHtmlFirmaPendiente = $@"
                            <div style='font-family: Arial, sans-serif; padding: 20px; color: #333;'>
                                <h2 style='color: #2563eb;'>QualityDoc — Control de Firma Asignado</h2>
                                <p>Estimado(a) <strong>{siguiente.Usuario.Nombre}</strong>,</p>
                                <p>La versión del documento ha sido validada por la fase anterior y se ha habilitado su turno en la cola de firmas:</p>
                                <table style='background: #f8f9fa; padding: 12px; width: 100%; border-radius: 6px; margin-bottom: 15px;'>
                                    <tr><td><strong>Documento:</strong></td><td>{versionData.Documento.Nombre}</td></tr>
                                    <tr><td><strong>Código:</strong></td><td>{versionData.Documento.Codigo}</td></tr>
                                    <tr><td><strong>Acción Requerida:</strong></td><td>{siguiente.TipoFirma}</td></tr>
                                </table>
                                <br/>
                                <a href='{linkVersion}' style='background-color: #2563eb; color: #fff; padding: 12px 24px; text-decoration: none; border-radius: 6px; font-weight: bold; display: inline-block;'>
                                    Revisar y Firmar Documento
                                </a>
                                <p style='font-size: 12px; color: #888; margin-top: 40px;'>Sistema de Gestión ISO 9001 - QualityDoc Polyglot</p>
                            </div>";

                            await _emailService.EnviarCorreoAsync(siguiente.Usuario.Email, $"Acción Requerida: Firma Pendiente — {versionData.Documento.Codigo}", cuerpoHtmlFirmaPendiente);
                        }
                    }
                    // ============================================================
                    // CONTEXTO C: LA VERSIÓN FUE RECHAZADA / CANCELADA
                    // ============================================================
                    else if (model.Accion == "Rechazado")
                    {
                        var involucradosAlerta = await _context.FlujosAprobacions
                            .Where(f => f.VersionId == versionData.Id && f.EstadoFirma == "Aprobado")
                            .Select(f => f.Usuario.Email)
                            .ToListAsync();

                        // EJECUCIÓN DIRECTA: Resolver el ID del Creador de la versión para obtener su Email
                        var correoCreador = await _context.Usuarios
                            .Where(u => u.Id == versionData.CreadoPor)
                            .Select(u => u.Email)
                            .FirstOrDefaultAsync();

                        if (!string.IsNullOrEmpty(correoCreador))
                        {
                            involucradosAlerta.Add(correoCreador);
                        }

                        string cuerpoHtmlCancelado = $@"
                        <div style='font-family: Arial, sans-serif; padding: 20px; color: #333;'>
                            <h2 style='color: #ef4444;'>QualityDoc — Flujo Interrumpido (Rechazo)</h2>
                            <p>Se le notifica que el flujo de aprobación para el siguiente documento ha sido rechazado por un dictaminador:</p>
                            <div style='background: #fef2f2; padding: 15px; border-left: 4px solid #ef4444; border-radius: 4px;'>
                                <strong>Documento:</strong> {docNombreCompleto}<br/>
                                <strong>Rechazado por:</strong> {flujoActual.Usuario.Nombre}<br/>
                                <strong>Motivo / Observaciones:</strong> <span style='color: #b91c1c;'>""{model.Comentarios}""</span>
                            </div>
                            <p style='margin-top: 15px;'>La versión actual ha mutado de estado. Se requiere inyectar una corrección física para reanudar el proceso.</p>                  
                            <br/>
                            <a href='{linkVersion}' style='background-color: #ef4444; color: #fff; padding: 12px 24px; text-decoration: none; border-radius: 6px; font-weight: bold; display: inline-block;'>
                                Ver Observaciones en Visor
                            </a>
                            <p style='font-size: 12px; color: #888; margin-top: 40px;'>Sistema de Gestión ISO 9001 - QualityDoc Polyglot</p>
                        </div>";

                        foreach (var email in involucradosAlerta.Distinct())
                        {
                            await _emailService.EnviarCorreoAsync(email, $"Rechazado: {versionData.Documento.Codigo}", cuerpoHtmlCancelado);
                        }
                    }
                }

                return RedirectToAction("VisualizarVersion", new { versionId = model.VersionId });
            }
            catch (Exception ex)
            {
                //await transaction.RollbackAsync(); // Evitamos bloqueos permanentes en el pool de SQL Server
                Console.WriteLine($"[FATAL ERROR] Falló el flujo de firmas o mensajería: {ex.Message}");
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
                .Include(v => v.Documento).ThenInclude(d => d.Departamento).ThenInclude(dep => dep.Compania)
                .FirstOrDefaultAsync(v => v.Id == model.VersionActualId);

            if (versionActual == null) return NotFound();

            // Nueva versión menor
            byte vMayor = versionActual.VersionMayor;
            byte vMenor = (byte)(versionActual.VersionMenor + 1);

            string extension = Path.GetExtension(model.ArchivoNuevo.FileName).ToLower();
            string rutaMinio = $"{versionActual.Documento.Departamento.Compania.Id}/{versionActual.Documento.DepartamentoId}/Nivel_{versionActual.Documento.Nivel.Numero}/{versionActual.Documento.Codigo}-v{vMayor}.{vMenor}{extension}";

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





        // ==========================================
        // 1. VISTA DE LISTADO (GET)
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> GestionarSolicitudes(int departamentoId)
        {



            //imprimir los claims y el departamentoId para debug
            Console.WriteLine($"[DEBUG] DepartamentoId recibido: {departamentoId}");
            //i
            // Validación Estricta: ¿El usuario actual tiene el Rol 1 (Admin) en este DepartamentoId?
            string claimRequerido = $"{departamentoId}:1";
            if (!User.Claims.Any(c => c.Type == "DeptRole" && c.Value == claimRequerido))
            {
                return Forbid(); // Bloqueo a nivel backend si alguien manipula la URL
            }

            //guardar el nombre

            var depto = await _context.Departamentos.FindAsync(departamentoId);
            if (depto == null) return NotFound();

            var solicitudes = await _context.SolicitudesRegistros
                .Include(s => s.Rol)
                .Where(s => s.DepartamentoId == departamentoId && s.Estado == "Pendiente")
                .Select(s => new SolicitudPendienteDto
                {
                    SolicitudId = s.Id,
                    Nombre = s.Nombre,
                    Email = s.Email,
                    NombreRolSugerido = s.Rol != null ? s.Rol.Nombre : "Desconocido",
                    FechaSolicitud = s.FechaSolicitud
                })
                .ToListAsync();

            var model = new GestionarSolicitudesViewModel
            {
                UsuarioNombre = User.FindFirstValue(ClaimTypes.Name) ?? "Usuario",
                DepartamentoActivoId = departamentoId,
                DepartamentoActivoNombre = depto.Nombre,
                RolActivoNombre = User.FindFirstValue("DeptRoleName") ?? "Administrador", // Asumimos que el nombre del rol también se guarda en los claims
                Solicitudes = solicitudes
            };

            return View(model);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcesarSolicitud(ProcesarSolicitudViewModel model)
        {
            if (!ModelState.IsValid) return BadRequest();

            // Recuperar la solicitud con seguimiento para actualizar su estado
            var solicitud = await _context.SolicitudesRegistros.FindAsync(model.SolicitudId);
            if (solicitud == null || solicitud.Estado != "Pendiente") return NotFound();

            // Re-validación de Seguridad: El admin debe tener permisos sobre el depto de ESTA solicitud
            string claimRequerido = $"{solicitud.DepartamentoId}:1";
            if (!User.Claims.Any(c => c.Type == "DeptRole" && c.Value == claimRequerido))
            {
                return Forbid();
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                if (model.Accion == "Aprobar")
                {
                    // A. Mover a la tabla de Usuarios (El password ya viene encriptado en BCrypt)
                    var nuevoUsuario = new Usuario
                    {
                        Nombre = solicitud.Nombre,
                        Email = solicitud.Email,
                        PasswordHash = solicitud.PasswordHash,
                        DepartamentoId = solicitud.DepartamentoId,
                        Activo = true,
                        CreadoEn = DateTime.UtcNow
                    };

                    _context.Usuarios.Add(nuevoUsuario);
                    await _context.SaveChangesAsync(); // Genera el nuevoUsuario.Id

                    // B. Asignar la matriz de permisos
                    var asignacionRol = new UsuariosRole
                    {
                        UsuarioId = nuevoUsuario.Id,
                        RolId = solicitud.RolId,
                        DepartamentoId = solicitud.DepartamentoId
                    };

                    _context.UsuariosRoles.Add(asignacionRol);

                    // C. Marcar la solicitud como aprobada
                    solicitud.Estado = "Aprobado";

                    // D. Notificar al usuario que su cuenta fue activada
                    string cuerpoHtml = $@"
                        <div style='font-family: sans-serif; padding: 20px;'>
                            <h2 style='color: #10b981;'>¡Solicitud Aprobada!</h2>
                            <p>Hola <strong>{solicitud.Nombre}</strong>,</p>
                            <p>Tu acceso al sistema QualityDoc ha sido autorizado por el administrador de tu departamento.</p>
                            <p>Ya puedes iniciar sesión con las credenciales que registraste.</p>
                            <br/>
                            <a href='{Request.Scheme}://{Request.Host}/Account/Login' style='background-color: #10b981; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px; font-weight: bold;'>Ir al Inicio de Sesión</a>
                        </div>";

                    await _emailService.EnviarCorreoAsync(solicitud.Email, "QualityDoc — Cuenta Activada", cuerpoHtml);
                }
                else if (model.Accion == "Rechazar")
                {
                    solicitud.Estado = "Rechazado";
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // Redirigir de vuelta a la lista de solicitudes de ese departamento
                return RedirectToAction("GestionarSolicitudes", new { departamentoId = solicitud.DepartamentoId });
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                TempData["ErrorSistema"] = "Ocurrió un error al procesar la solicitud.";
                return RedirectToAction("GestionarSolicitudes", new { departamentoId = solicitud.DepartamentoId });
            }
        }

        // Agrega este action dentro de tu DashboardController existente

        [HttpGet]
        public async Task<IActionResult> Analiticas(int departamentoId)
        {
            // Validación de claim igual al patrón existente
            string claimRequerido = $"{departamentoId}:1";
            if (!User.Claims.Any(c => c.Type == "DeptRole" && c.Value == claimRequerido))
                return Forbid();

            var depto = await _context.Departamentos.FindAsync(departamentoId);
            if (depto == null) return NotFound();

            var model = new DashboardAnaliticasViewModel
            {
                UsuarioNombre = User.Identity?.Name ?? string.Empty,
                RolActivoNombre = User.Claims.FirstOrDefault(c => c.Type == "RolActivo")?.Value ?? "Administrador",
                DepartamentoActivoId = departamentoId,
                DepartamentoActivoNombre = depto.Nombre,

                ActividadSemanal = await _context.ActividadSemanal
                    .Where(x => x.DepartamentoId == departamentoId)
                    .OrderBy(x => x.InicioSemana)
                    .ToListAsync(),

                DocumentosMasVersiones = await _context.DocumentosMasVersiones
                    .Where(x => x.DepartamentoId == departamentoId)
                    .OrderByDescending(x => x.TotalVersiones)
                    .Take(10)
                    .ToListAsync(),

                FirmasPendientes = await _context.FirmasPendientes
                    .Where(x => x.DepartamentoId == departamentoId)
                    .OrderBy(x => x.VersionCreadaEn)
                    .ToListAsync(),

                UsuariosActivos = await _context.UsuariosActivos
                    .Where(x => x.DepartamentoId == departamentoId)
                    .OrderByDescending(x => x.TotalFirmas)
                    .Take(10)
                    .ToListAsync()
            };

            //imprimir los valores de ActividadSemanal, DocumentosMasVersiones, FirmasPendientes y UsuariosActivos para debug
            Console.WriteLine($"[DEBUG] ActividadSemanal: {model.ActividadSemanal.Count} registros");
            Console.WriteLine($"[DEBUG] DocumentosMasVersiones: {model.DocumentosMasVersiones.Count} registros");
            Console.WriteLine($"[DEBUG] FirmasPendientes: {model.FirmasPendientes.Count} registros");
            Console.WriteLine($"[DEBUG] UsuariosActivos: {model.UsuariosActivos.Count} registros");

            ViewData["Title"] = "Analíticas";
            ViewData["ActiveNav"] = "analiticas";
            ViewData["UsuarioNombre"] = model.UsuarioNombre;
            ViewData["RolActivoNombre"] = model.RolActivoNombre;
            ViewData["Departamento"] = model.DepartamentoActivoNombre;

            return View(model);
        }

    }
}