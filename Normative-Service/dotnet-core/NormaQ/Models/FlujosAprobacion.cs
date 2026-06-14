using System;
using System.Collections.Generic;

namespace NormaQ.Models;

public partial class FlujosAprobacion
{
    public int Id { get; set; }

    public int VersionId { get; set; }

    public int UsuarioId { get; set; }

    public string TipoFirma { get; set; } = null!;

    public byte Orden { get; set; }

    public string EstadoFirma { get; set; } = null!;

    public string? Comentarios { get; set; }

    public DateTime? FechaFirma { get; set; }

    public virtual Usuario Usuario { get; set; } = null!;

    public virtual VersionesDocumento Version { get; set; } = null!;
}
