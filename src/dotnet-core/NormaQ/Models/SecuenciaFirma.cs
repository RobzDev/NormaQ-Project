using System;
using System.Collections.Generic;

namespace NormaQ.Models;

public partial class SecuenciaFirma
{
    public int Id { get; set; }

    public int DocumentoId { get; set; }

    public int RolId { get; set; }

    public string TipoFirma { get; set; } = null!;

    public byte Orden { get; set; }

    public virtual Documento Documento { get; set; } = null!;

    public virtual Role Rol { get; set; } = null!;
}
