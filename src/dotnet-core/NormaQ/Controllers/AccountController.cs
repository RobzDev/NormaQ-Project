using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using NormaQ.Models;
using NormaQ.ViewModels;
using System.Linq;
using BCrypt.Net;
using NormaQ.Data;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using System.Text.Json;
using NormaQ.Services;

namespace NormaQ.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IDatabase _redis; // Representa la DB de Redis
        private readonly IEmailService _emailService;
        // Inyección de dependencias: ASP.NET nos pasa el DbContext configurado
        public AccountController(AppDbContext context, IConnectionMultiplexer redis, IEmailService emailService)
        {
            _context = context;
            _redis = redis.GetDatabase(); // Obtenemos la conexión a la DB por defecto (0)
            _emailService = emailService;
        }

        // ==========================================
        // LOGIN (GET y POST)
        // ==========================================
        [HttpGet]
        public IActionResult Login()
        {
            return View(); // Retorna Views/Account/Login.cshtml
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            // ==========================================
            // 1. CONTROL DE ACCESO: SALA DE ESPERA
            // ==========================================

            // Verificar si el correo está en la tabla de solicitudes
            var solicitud = await _context.SolicitudesRegistros
                .FirstOrDefaultAsync(s => s.Email == model.Email);

            if (solicitud != null)
            {
                if (solicitud.Estado == "Pendiente")
                {
                    // Bloqueo 1: La cuenta aún está en evaluación por el Admin
                    return RedirectToAction("RegistroPendiente", "Account");
                }
                else if (solicitud.Estado == "Rechazado")
                {
                    // Bloqueo 2: El Admin denegó el acceso a este usuario
                    ModelState.AddModelError(string.Empty, "Tu solicitud de acceso fue rechazada por el administrador del departamento.");
                    return View(model);
                }
                // Si es "Aprobado", el flujo simplemente continúa porque sus datos ya deberían 
                // haber sido migrados a la tabla Usuarios.
            }

            // ==========================================
            // 2. AUTENTICACIÓN PRINCIPAL
            // ==========================================

            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == model.Email);

            if (usuario != null && usuario.Activo && BCrypt.Net.BCrypt.Verify(model.Password, usuario.PasswordHash))
            {
                // 3. Ejecución: Consultar la matriz de permisos (Roles y Departamentos del usuario)
                var permisos = await _context.UsuariosRoles
                    .Include(ur => ur.Departamento)
                    .Where(ur => ur.UsuarioId == usuario.Id)
                    .ToListAsync();

                

                // 4. Ejecución: Construcción de Claims
                var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new Claim(ClaimTypes.Name, usuario.Nombre),
            new Claim(ClaimTypes.Email, usuario.Email),
            // Claim principal del departamento base del usuario
            new Claim("DepartamentoBaseId", usuario.DepartamentoId.ToString())
        };
        

                // Inyectamos la matriz de permisos como Claims personalizados
                // Formato del Claim: "DeptID:RolID"
                foreach (var permiso in permisos)
                {
                    claims.Add(new Claim("DeptRole", $"{permiso.DepartamentoId}:{permiso.RolId}"));
                }

                // 5. Ejecución: Creación de la Identidad y el Principal
                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true,
                    IssuedUtc = DateTimeOffset.UtcNow
                };

                // 6. Ejecución: Emisión de la Cookie
                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);

                // ==========================================
                // 7. ENRUTAMIENTO DINÁMICO (SSO C# vs PHP)
                // ==========================================

                // Verificamos si tiene el rol de 'Operario' (ID 5) en algún departamento
                bool esOperario = permisos.Any(p => p.RolId == 5);

                if (esOperario)
                {
                    string ssoToken = Guid.NewGuid().ToString();

                    // Llenar el payload con datos reales del usuario
                    var payload = new
                    {
                        UsuarioId = usuario.Id,
                        Nombre = usuario.Nombre,
                        Email = usuario.Email,
                        DepartamentoOperario = permisos
                            .Where(p => p.RolId == 5)
                            .Select(p => new
                            {
                                p.DepartamentoId,
                                Nombre = p.Departamento.Nombre
                            })
                            .FirstOrDefault()
                    };

                    string jsonPayload = System.Text.Json.JsonSerializer.Serialize(payload);

                    // Guardar en Redis usando StackExchange.Redis
                    await _redis.StringSetAsync(
                        $"SSO_TOKEN_{ssoToken}",
                        jsonPayload,
                        TimeSpan.FromSeconds(45)
                    );

                    // Redirigir a Laravel
                    return Redirect($"/php-app/hub?token={ssoToken}");
                }
                else
                {
                    // Redirección al portal administrativo de C# (QualityDoc)
                    return RedirectToAction("Index", "Dashboard");
                }
            }

            // Falla la autenticación por correo inexistente, inactivo o contraseña incorrecta
            ModelState.AddModelError(string.Empty, "Credenciales inválidas o cuenta inexistente.");
            return View(model);
        }
        // ==========================================
        // REGISTRO (GET y POST) - Usuario INIT
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> Register()
        {
            var model = new RegisterViewModel();

            // 1. Ejecución: Carga de Compañías y Roles globales
            model.Companias = await _context.Companias
                .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Nombre })
                .ToListAsync();

            model.Roles = await _context.Roles
                .Select(r => new SelectListItem { Value = r.Id.ToString(), Text = r.Nombre })
                .ToListAsync();

            // La lista de Departamentos inicia vacía, se llenará vía AJAX en el cliente
            model.Departamentos = new List<SelectListItem>();

            return View(model);
        }

        // 2. Ejecución: Endpoint AJAX para alimentar el segundo Select
        [HttpGet]
        public async Task<IActionResult> GetDepartamentosPorCompania(int companiaId)
        {
            if (companiaId <= 0) return Json(new List<object>());

            var departamentos = await _context.Departamentos
                .Where(d => d.CompaniaId == companiaId)
                .Select(d => new { value = d.Id, text = d.Nombre })
                .ToListAsync();

            return Json(departamentos);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            // ==========================================
            // PASO A: VALIDACIÓN ESTRICTA Y SEGURIDAD
            // ==========================================

            // 1. Ejecución: Prevenir inyección de departamentos huérfanos
            if (model.CompaniaId > 0 && model.DepartamentoId > 0)
            {
                bool deptoValido = await _context.Departamentos
                    .AnyAsync(d => d.Id == model.DepartamentoId && d.CompaniaId == model.CompaniaId);

                if (!deptoValido)
                {
                    ModelState.AddModelError("DepartamentoId", "El departamento seleccionado no pertenece a la compañía indicada.");
                }
            }

            if (!ModelState.IsValid)
            {
                return await RecargarCatalogosYDevolverVista(model);
            }

            // 2. Ejecución: Verificación de duplicidad cruzada
            bool existeUsuario = await _context.Usuarios.AnyAsync(u => u.Email == model.Email);
            bool existeSolicitud = await _context.SolicitudesRegistros.AnyAsync(s => s.Email == model.Email && s.Estado == "Pendiente");

            if (existeUsuario || existeSolicitud)
            {
                ModelState.AddModelError("Email", "Este correo ya pertenece a una cuenta activa o tiene una solicitud en proceso.");
                return await RecargarCatalogosYDevolverVista(model);
            }

            // 3. Ejecución: Encriptado e Inserción (Solo insertamos DepartamentoId en BD)
            var nuevaSolicitud = new SolicitudesRegistro
            {
                Nombre = model.Nombre,
                Email = model.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
                DepartamentoId = model.DepartamentoId, // La DB infiere la compañía automáticamente a través de este FK
                RolId = model.RolId,
                Estado = "Pendiente",
                FechaSolicitud = DateTime.UtcNow
            };

            _context.SolicitudesRegistros.Add(nuevaSolicitud);
            await _context.SaveChangesAsync();

            // ==========================================
            // PASO B: BÚSQUEDA DEL ADMINISTRADOR DE ESE DEPARTAMENTO Y NOTIFICACIÓN
            // ==========================================

            // (Esta sección permanece exactamente igual a tu código original)
            var depto = await _context.Departamentos.FindAsync(model.DepartamentoId);
            string nombreDepto = depto?.Nombre ?? "Área Desconocida";

            var correosAdmins = await _context.UsuariosRoles
                .Include(ur => ur.Usuario)
                .Where(ur => ur.DepartamentoId == model.DepartamentoId && ur.RolId == 1 && ur.Usuario.Activo)
                .Select(ur => ur.Usuario.Email)
                .ToListAsync();

            string linkAprobacion = Url.Action("GestionarSolicitudes", "Dashboard", new { departamentoId = model.DepartamentoId }, Request.Scheme) ?? string.Empty;

            string cuerpoHtml = $@"
        <div style='font-family: Arial, sans-serif; padding: 20px; color: #333;'>
            <h2 style='color: #2563eb;'>QualityDoc — Nueva Solicitud de Acceso</h2>
            <p>Estimado Administrador,</p>
            <p>Un usuario ha solicitado unirse al departamento de <strong>{nombreDepto}</strong>.</p>
            <table style='background: #f8f9fa; padding: 15px; border-radius: 5px; width: 100%; max-width: 500px;'>
                <tr><td><strong>Nombre:</strong></td><td>{nuevaSolicitud.Nombre}</td></tr>
                <tr><td><strong>Email:</strong></td><td>{nuevaSolicitud.Email}</td></tr>
            </table>
            <br/>
            <a href='{linkAprobacion}' style='background-color: #2563eb; color: #ffffff; padding: 10px 20px; text-decoration: none; border-radius: 5px; font-weight: bold;'>
                Revisar Solicitud
            </a>
            <p style='font-size: 12px; color: #888; margin-top: 30px;'>Sistema de Gestión ISO 9001 - QualityDoc Polyglot</p>
        </div>";

            if (correosAdmins.Any())
            {
                foreach (var emailAdmin in correosAdmins)
                {
                    await _emailService.EnviarCorreoAsync(emailAdmin, $"Solicitud de Registro - {nombreDepto}", cuerpoHtml);
                }
            }
            else
            {
                await _emailService.EnviarCorreoAsync("rcespinoza04@gmail.com", $"¡ALERTA! Solicitud Huérfana - {nombreDepto}", cuerpoHtml);
            }

            return RedirectToAction("RegistroPendiente");
        }

        // 4. Ejecución: Método auxiliar actualizado
        private async Task<IActionResult> RecargarCatalogosYDevolverVista(RegisterViewModel model)
        {
            model.Companias = await _context.Companias
                .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Nombre })
                .ToListAsync();

            model.Roles = await _context.Roles
                .Select(r => new SelectListItem { Value = r.Id.ToString(), Text = r.Nombre })
                .ToListAsync();

            // Si falló el modelo pero el usuario ya había seleccionado una compañía, recargamos sus departamentos
            if (model.CompaniaId > 0)
            {
                model.Departamentos = await _context.Departamentos
                    .Where(d => d.CompaniaId == model.CompaniaId)
                    .Select(d => new SelectListItem { Value = d.Id.ToString(), Text = d.Nombre })
                    .ToListAsync();
            }
            else
            {
                model.Departamentos = new List<SelectListItem>();
            }

            return View(model);
        }

        
        [HttpGet]
        public IActionResult RegistroPendiente()
        {
            return View();
        }

        [HttpGet]
        public IActionResult AccessDenied(string? reason = null, string? expected = null, string? actual = null)
        {
            ViewData["Reason"] = reason;
            ViewData["Expected"] = expected;
            ViewData["Actual"] = actual;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            // Cierra la sesión - esto automáticamente:
            // 1. Elimina la cookie "NormaQ_AuthTicket"
            // 2. Limpia el ClaimsPrincipal del usuario
            // 3. Invalida la autenticación
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            // Redirige al login (o a donde prefieras)
            return RedirectToAction("Login", "Account");
        }



        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            string emailKey = model.Email.ToLower().Trim();
            string rateLimitKey = $"ratelimit:pwdreset:{emailKey}";

            // 1. Ejecución: Verificación de Rate Limit en Redis
            // Evita que un bot inunde la bandeja de correo del usuario
            bool isInRateLimit = await _redis.KeyExistsAsync(rateLimitKey);
            if (isInRateLimit)
            {
                // Se bloquea silenciosamente para el usuario, pero con un mensaje genérico
                TempData["Mensaje"] = "Si el correo está registrado, recibirás un enlace de recuperación. Por favor revisa tu bandeja en unos minutos.";
                return RedirectToAction("Login");
            }

            // 2. Aplicar el Rate Limit (Bloqueo por 3 minutos)
            await _redis.StringSetAsync(rateLimitKey, "1", TimeSpan.FromMinutes(3));

            // 3. Ejecución: Buscar usuario real
            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == emailKey);

            // Si el usuario existe y está activo, procedemos con la lógica real
            if (usuario != null && usuario.Activo)
            {
                // 4. Generación del Token y guardado temporal
                string token = Guid.NewGuid().ToString("N"); // String alfanumérico sin guiones
                string tokenKey = $"pwdreset:token:{token}";

                // Guardamos el ID del usuario ligado al token, con un TTL de 15 minutos
                await _redis.StringSetAsync(tokenKey, usuario.Id.ToString(), TimeSpan.FromMinutes(15));

                // 5. Construcción del enlace y envío de correo
                string resetLink = Url.Action("ResetPassword", "Account", new { token = token }, Request.Scheme) ?? string.Empty;

                string cuerpoHtml = $@"
            <div style='font-family: Arial, sans-serif; padding: 20px;'>
                <h2 style='color: #4f46e5;'>Recuperación de Contraseña</h2>
                <p>Hola <strong>{usuario.Nombre}</strong>,</p>
                <p>Hemos recibido una solicitud para restablecer tu contraseña en QualityDoc.</p>
                <p>Haz clic en el siguiente enlace para crear una nueva (este enlace expira en 15 minutos):</p>
                <br/>
                <a href='{resetLink}' style='background-color: #4f46e5; color: white; padding: 12px 24px; text-decoration: none; border-radius: 6px; font-weight: bold;'>Restablecer Contraseña</a>
                <br/><br/>
                <p style='font-size: 12px; color: #666;'>Si no solicitaste esto, puedes ignorar este correo de forma segura.</p>
            </div>";

                await _emailService.EnviarCorreoAsync(usuario.Email, "QualityDoc - Recuperación de Contraseña", cuerpoHtml);
            }

            // Respuesta genérica siempre (seguridad)
            TempData["Mensaje"] = "Si el correo está registrado, recibirás un enlace de recuperación. Por favor revisa tu bandeja de entrada o spam.";
            return RedirectToAction("Login");
        }

        [HttpGet]
        public async Task<IActionResult> ResetPassword(string token)
        {
            if (string.IsNullOrEmpty(token)) return RedirectToAction("Login");

            // 1. Ejecución: Verificar que el token exista y no haya expirado en Redis
            string tokenKey = $"pwdreset:token:{token}";
            var userIdString = await _redis.StringGetAsync(tokenKey);

            if (!userIdString.HasValue)
            {
                // El token no existe o ya expiró por su TTL
                TempData["Error"] = "El enlace de recuperación es inválido o ha expirado. Por favor solicita uno nuevo.";
                return RedirectToAction("ForgotPassword");
            }

            // Se muestra la vista pasando el token oculto
            var model = new ResetPasswordViewModel { Token = token };
            return View("~/Views/Account/ResetPassword.cshtml", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid) return View("~/Views/Account/ResetPassword.cshtml", model);

            string tokenKey = $"pwdreset:token:{model.Token}";

            // 2. Ejecución: Doble verificación del token al hacer POST
            var userIdString = await _redis.StringGetAsync(tokenKey);

            if (!userIdString.HasValue)
            {
                TempData["Error"] = "El enlace de recuperación ha expirado durante el proceso.";
                return RedirectToAction("ForgotPassword");
            }

            int userId = int.Parse(userIdString.ToString());
            var usuario = await _context.Usuarios.FindAsync(userId);

            if (usuario != null)
            {
                // 3. Ejecución: Actualizar BD con la nueva contraseña hasheada
                usuario.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
                await _context.SaveChangesAsync();

                // 4. Ejecución: Invalidar (Borrar) el token inmediatamente después de usarse
                await _redis.KeyDeleteAsync(tokenKey);

                TempData["Exito"] = "Tu contraseña ha sido actualizada correctamente. Ya puedes iniciar sesión.";
                return RedirectToAction("Login");
            }

            return RedirectToAction("Login");
        }




    }
}