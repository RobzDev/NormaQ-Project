using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;
using System.Text.Json;

namespace NormaQ.Controllers
{
    [Route("api/[controller]")]
    [ApiController] // Indica que es una API, formatea respuestas a JSON automáticamente
    public class SsoController : ControllerBase
    {
        private readonly IDatabase _redis;

        public SsoController(IConnectionMultiplexer redis)
        {
            _redis = redis.GetDatabase();
        }

        [HttpGet("validate")]
        public async Task<IActionResult> ValidateToken([FromQuery] string token)
        {
            string cacheKey = $"SSO_TOKEN_{token}";

            // 1. Ejecución: Obtener el valor
            var result = await _redis.StringGetAsync(cacheKey);

            // 2. Validación técnica: RedisValue.IsNullOrEmpty maneja nulos y strings vacíos
            if (result.IsNullOrEmpty)
            {
                return Unauthorized(new { message = "Token expirado o inválido." });
            }

            // 3. Ejecución: Borrado (Single-Use)
            await _redis.KeyDeleteAsync(cacheKey);

            // 4. Retorno: El valor obtenido de Redis es un string JSON
            return Content(result.ToString(), "application/json");
        }
    }
}