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

namespace NormaQ.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IDatabase _redis; // Representa la DB de Redis
        // Inyección de dependencias: ASP.NET nos pasa el DbContext configurado
        public AccountController(AppDbContext context, IConnectionMultiplexer redis)
        {
            _context = context;
            _redis = redis.GetDatabase(); // Obtenemos la conexión a la DB por defecto (0)
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
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            // 1. Ejecución: Buscar usuario
            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == model.Email);

            if (usuario != null && usuario.Activo && BCrypt.Net.BCrypt.Verify(model.Password, usuario.PasswordHash))
            {
                // 2. Ejecución: Consultar la matriz de permisos (Roles y Departamentos del usuario)
                var permisos = await _context.UsuariosRoles
       .Include(ur => ur.Departamento)
       .Where(ur => ur.UsuarioId == usuario.Id)
       .ToListAsync();

                // 3. Ejecución: Construcción de Claims
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

                // 4. Ejecución: Creación de la Identidad y el Principal
                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true, // Mantiene la sesión si se cierra el navegador (opcional, puedes ligarlo a un checkbox "Recuérdame")
                    IssuedUtc = DateTimeOffset.UtcNow
                };

                // 5. Ejecución: Emisión de la Cookie
                await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

                // ==========================================
                // 6. Lógica de Redirección (C# vs PHP)
                // ==========================================

                // Verificamos si tiene el rol de 'Operario' (ID 5 según tu DDL) en algún departamento
                bool esOperario = permisos.Any(p => p.RolId == 5);

                if (esOperario)
                {
                    string ssoToken = Guid.NewGuid().ToString();

                    // EJECUCIÓN: Llenar el payload con datos reales del usuario
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

                    string jsonPayload = JsonSerializer.Serialize(payload);

                    // Guardar en Redis usando StackExchange.Redis
                    await _redis.StringSetAsync(
                        $"SSO_TOKEN_{ssoToken}",
                        jsonPayload,
                        TimeSpan.FromSeconds(45)
                    );

                    // Redirigir a Laravel
                    return Redirect($"http://localhost/php-app/hub?token={ssoToken}");
                }
                else
                {
                    // Redirección al portal administrativo de C#
                    return RedirectToAction("Index", "Dashboard"); // Controlador que crearemos luego
                }
            }

            ModelState.AddModelError(string.Empty, "Credenciales inválidas.");
            return View(model);
        }
        // ==========================================
        // REGISTRO (GET y POST) - Usuario INIT
        // ==========================================
        [HttpGet]
        public IActionResult Register()
        {
            // 1. Instanciamos el ViewModel
            var model = new RegisterViewModel();

            // 2. Ejecución de consulta LINQ a la tabla Departamentos
            // Seleccionamos solo ID y Nombre para optimizar la carga
            model.Departamentos = _context.Departamentos
            .Where(d => d.Activo == true) // Solo departamentos activos
            .Select(d => new SelectListItem
            {
                Value = d.Id.ToString(),
                Text = d.Nombre
            })
            .ToList();

            // 3. Pasamos el modelo ya cargado a la vista
            return View(model);
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





        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // 1. Ejecución: Verificar si el correo ya existe
            if (_context.Usuarios.Any(u => u.Email == model.Email))
            {
                ModelState.AddModelError("Email", "Este correo ya está registrado.");
                return View(model);
            }

            // 2. Ejecución: Encriptar la contraseña usando BCrypt
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(model.Password);

            // 3. Ejecución: Mapear el ViewModel a la Entidad
            var nuevoUsuario = new Usuario
            {
                Nombre = model.Nombre,
                Email = model.Email,
                PasswordHash = passwordHash,
                DepartamentoId = model.DepartamentoId,
                Activo = true // Por defecto activo para el usuario init
            };

            // 4. Ejecución: Guardar en la base de datos
            _context.Usuarios.Add(nuevoUsuario);
            _context.SaveChanges();

            // Redirigir al login tras un registro exitoso
            return RedirectToAction("Login");
        }
    }
}