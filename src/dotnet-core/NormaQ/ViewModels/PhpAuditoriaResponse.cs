
using System.Text.Json.Serialization;

namespace NormaQ.ViewModels
{


public class PhpAuditoriaResponse
{
    [JsonPropertyName("results")]
    public List<PhpAuditoriaItem> Results { get; set; } = [];
}

public class PhpAuditoriaItem
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("fecha_hora")]
    public string FechaHora { get; set; } = string.Empty;

    [JsonPropertyName("usuario_id")]
    public int UsuarioId { get; set; }

    [JsonPropertyName("usuario_nombre")]
    public string UsuarioNombre { get; set; } = string.Empty;

    [JsonPropertyName("documento_codigo")]
    public string DocumentoCodigo { get; set; } = string.Empty;

    [JsonPropertyName("documento_nombre")]
    public string DocumentoNombre { get; set; } = string.Empty;

    [JsonPropertyName("version_documento")]
    public string VersionDocumento { get; set; } = string.Empty;

    [JsonPropertyName("accion")]
    public string Accion { get; set; } = string.Empty;

    [JsonPropertyName("ip_origen")]
    public string IpOrigen { get; set; } = string.Empty;
}


}

