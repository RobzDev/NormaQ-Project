using Microsoft.EntityFrameworkCore;
using NormaQ.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


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


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();








app.Run();
