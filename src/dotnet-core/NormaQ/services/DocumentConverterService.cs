using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace NormaQ.Services
{
    public class DocumentConverterService
    {
        private readonly ILogger<DocumentConverterService> _logger;

        public DocumentConverterService(ILogger<DocumentConverterService> logger)
        {
            _logger = logger;
        }

        public async Task<Stream> ConvertDocxToPdfAsync(Stream docxStream)
        {
            string uniqueId = Guid.NewGuid().ToString();
            string tempDir = Path.Combine(Path.GetTempPath(), $"docx_conv_{uniqueId}");
            string profileDir = Path.Combine(Path.GetTempPath(), $"docx_prof_{uniqueId}");

            try
            {
                Directory.CreateDirectory(tempDir);
                Directory.CreateDirectory(profileDir);

                string tmpDocx = Path.Combine(tempDir, "document.docx");

                // Escribir el stream al archivo temporal docx
                using (var fileStream = new FileStream(tmpDocx, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    await docxStream.CopyToAsync(fileStream);
                }

                var envProfilePath = $"file://{profileDir}";

                var startInfo = new ProcessStartInfo
                {
                    FileName = "libreoffice",
                    Arguments = $"--headless \"-env:UserInstallation={envProfilePath}\" --convert-to pdf --outdir \"{tempDir}\" \"{tmpDocx}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                startInfo.EnvironmentVariables["FONTCONFIG_PATH"] = "/tmp";
                startInfo.EnvironmentVariables["HOME"] = "/tmp";

                using (var process = new Process { StartInfo = startInfo })
                {
                    process.Start();
                    string stdOut = await process.StandardOutput.ReadToEndAsync();
                    string stdErr = await process.StandardError.ReadToEndAsync();
                    await process.WaitForExitAsync();

                    if (process.ExitCode != 0)
                    {
                        _logger.LogError("LibreOffice falló con código {Code}. StdOut: {StdOut}, StdErr: {StdErr}", process.ExitCode, stdOut, stdErr);
                        throw new Exception($"LibreOffice conversión falló con código: {process.ExitCode}. Detalle: {stdErr}");
                    }
                }

                string pdfPath = Path.Combine(tempDir, "document.pdf");
                if (!File.Exists(pdfPath))
                {
                    throw new FileNotFoundException("El archivo PDF convertido no fue generado por LibreOffice.");
                }

                // Leer el archivo PDF a memoria para poder limpiar los temporales inmediatamente
                var pdfBytes = await File.ReadAllBytesAsync(pdfPath);
                return new MemoryStream(pdfBytes);
            }
            finally
            {
                // Bloque de limpieza
                try
                {
                    if (Directory.Exists(tempDir))
                        Directory.Delete(tempDir, true);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "No se pudo limpiar el directorio temporal: {TempDir}", tempDir);
                }

                try
                {
                    if (Directory.Exists(profileDir))
                        Directory.Delete(profileDir, true);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "No se pudo limpiar el perfil temporal de LibreOffice: {ProfileDir}", profileDir);
                }
            }
        }
    }
}
