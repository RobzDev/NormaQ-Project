using System.Threading.Tasks;

namespace NormaQ.Services
{
    public interface IEmailService
    {
        Task EnviarCorreoAsync(string correoDestino, string asunto, string cuerpoHtml);
    }
}