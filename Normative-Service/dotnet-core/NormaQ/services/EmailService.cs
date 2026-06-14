using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;
using System;
using System.Threading.Tasks;

namespace NormaQ.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task EnviarCorreoAsync(string correoDestino, string asunto, string cuerpoHtml)
        {
            var email = new MimeMessage();
            email.From.Add(new MailboxAddress("QualityDoc System", _config["Email:Sender"]));
            email.To.Add(MailboxAddress.Parse(correoDestino));
            email.Subject = asunto;

            var bodyBuilder = new BodyBuilder { HtmlBody = cuerpoHtml };
            email.Body = bodyBuilder.ToMessageBody();

            using var smtp = new SmtpClient();
            try
            {
                // EJECUCIÓN CAMBIO GMAIL: Puerto 465 exige SslOnConnect (SSL Implícito)
                await smtp.ConnectAsync(
                    _config["Email:Host"],
                    int.Parse(_config["Email:Port"]!),
                    SecureSocketOptions.SslOnConnect
                );

                // Autenticación con tu usuario y la contraseña de aplicación de 16 letras
                await smtp.AuthenticateAsync(_config["Email:Username"], _config["Email:Password"]);

                await smtp.SendAsync(email);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GMAIL SMTP ERROR] {ex.Message}");
                throw;
            }
            finally
            {
                await smtp.DisconnectAsync(true);
            }
        }
    }
}