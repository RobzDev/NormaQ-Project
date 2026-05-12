using Microsoft.EntityFrameworkCore;
using NormaQ.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using StackExchange.Redis;
using Amazon.S3;
using NormaQ.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
var redisConnectionString = "redis:6379"; 
var multiplexer = ConnectionMultiplexer.Connect(redisConnectionString);

// 2. Inyectar como Singleton para que todo el proyecto lo use
builder.Services.AddSingleton<IConnectionMultiplexer>(multiplexer);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));




builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        // Ruta a la que .NET redirigirá automáticamente si un usuario sin sesión intenta acceder a algo privado
        options.LoginPath = "/Account/Login";
        
        // Ruta a la que redirigirá si el usuario tiene sesión, pero sus Claims (Roles/Depto) no le dan permiso
        options.AccessDeniedPath = "/Account/AccessDenied"; 
        
        // Tiempo de vida de la cookie de sesión (Ejemplo: 8 horas para una jornada laboral)
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        
        // Renueva automáticamente el tiempo de la cookie si el usuario sigue navegando a la mitad de su vida útil
        options.SlidingExpiration = true; 
        
        // Seguridad estricta
        options.Cookie.HttpOnly = true; // Evita que JavaScript (XSS) pueda leer la cookie
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // Obliga a que solo viaje por HTTPS
        options.Cookie.SameSite = SameSiteMode.Strict; // Previene ataques CSRF
        options.Cookie.Name = "NormaQ_AuthTicket"; // Nombre personalizado de la cookie en el navegador
    });

builder.Services.AddControllersWithViews();


// MinIO
var minioConfig = builder.Configuration.GetSection("MinIO");
builder.Services.AddSingleton<IAmazonS3>(_ => new AmazonS3Client(
    minioConfig["AccessKey"],
    minioConfig["SecretKey"],
    new AmazonS3Config
    {
        ServiceURL = $"http://{minioConfig["Endpoint"]}",
        ForcePathStyle = true,
        UseHttp = true
    }
));
builder.Services.AddSingleton<MinioService>();

var app = builder.Build();

var logger = app.Services.GetRequiredService<ILogger<Program>>();
const int maxMigrationAttempts = 10;

for (var attempt = 1; attempt <= maxMigrationAttempts; attempt++)
{
    try
    {
        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;
        var context = services.GetRequiredService<AppDbContext>();

        // Reintenta migraciones para cubrir el tiempo de arranque de SQL Server en Docker.
        context.Database.Migrate();
        string seedingSql = File.ReadAllText("/scripts/sqlserver/seeding.sql");
        context.Database.ExecuteSqlRaw(seedingSql);

        logger.LogInformation("Migraciones aplicadas correctamente en el intento {Attempt}.", attempt);
        break;
    }
    catch (Exception ex) when (attempt < maxMigrationAttempts)
    {
        var delay = TimeSpan.FromSeconds(Math.Min(attempt * 2, 15));
        logger.LogWarning(ex,
            "No fue posible conectar a SQL Server para ejecutar migraciones (intento {Attempt}/{MaxAttempts}). Reintentando en {DelaySeconds}s...",
            attempt,
            maxMigrationAttempts,
            delay.TotalSeconds);

        Thread.Sleep(delay);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Ocurrió un error al migrar la base de datos.");
    }
}


// Inicializar bucket al arrancar
using (var scope = app.Services.CreateScope())
{
    var minio = scope.ServiceProvider.GetRequiredService<MinioService>();
    await minio.EnsureBucketExistsAsync();
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication(); // ¿Quién eres? (Lee y desencripta la cookie 'NormaQ_AuthTicket')
app.UseAuthorization();  // ¿Qué puedes hacer? (Verifica los Claims contra las Policies que crearemos)

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();








app.Run();
