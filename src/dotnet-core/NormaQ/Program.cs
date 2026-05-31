using Microsoft.EntityFrameworkCore;
using NormaQ.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using StackExchange.Redis;
using Amazon.S3;
using NormaQ.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddHealthChecks();


builder.Services.AddSingleton<IConnectionMultiplexer>(
    ConnectionMultiplexer.Connect(builder.Configuration["Redis:ConnectionString"]!)
);
builder.Services.AddSingleton<RedisPublisherService>();

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
        options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
            ? CookieSecurePolicy.SameAsRequest
            : CookieSecurePolicy.Always; // En desarrollo permite HTTP local; en producción exige HTTPS
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

builder.Services.AddScoped<IEmailService, EmailService>();

var app = builder.Build();

app.MapHealthChecks("/health");

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
        var batches = seedingSql
        .Split(["\nGO", "\r\nGO"], StringSplitOptions.RemoveEmptyEntries)
        .Select(b => b.Trim())
        .Where(b => !string.IsNullOrWhiteSpace(b));

        foreach (var batch in batches)
        {
            context.Database.ExecuteSqlRaw(batch);
        }

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

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseRouting();

app.UseAuthentication(); // ¿Quién eres? (Lee y desencripta la cookie 'NormaQ_AuthTicket')
app.UseAuthorization();  // ¿Qué puedes hacer? (Verifica los Claims contra las Policies que crearemos)

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();








app.Run();
