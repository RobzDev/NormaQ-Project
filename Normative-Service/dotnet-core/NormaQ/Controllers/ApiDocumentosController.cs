using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NormaQ.Data;

namespace NormaQ.Controllers;

[ApiController]
[Route("api/documentos")]
public class ApiDocumentosController : ControllerBase
{
    private readonly AppDbContext _context;

    public ApiDocumentosController(AppDbContext context)
    {
        _context = context;
    }

    // Python consulta un documento aprobado específico por version_id
    [HttpGet("{versionId}")]
    public async Task<IActionResult> GetByVersionId(int versionId)
    {
        var doc = await _context.DocumentosAprobados
            .FirstOrDefaultAsync(d => d.VersionId == versionId);

        if (doc == null)
            return NotFound(new { message = $"No se encontró versión aprobada con id {versionId}" });

        return Ok(doc);
    }

    // Python consulta todos los aprobados (para sync_check al arrancar)
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var docs = await _context.DocumentosAprobados.ToListAsync();
        return Ok(docs);
    }

    
}