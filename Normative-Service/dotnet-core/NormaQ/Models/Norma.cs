using System;
using System.Collections.Generic;

namespace NormaQ.Models;

public partial class Norma
{
    public int Id { get; set; }

    public string Codigo { get; set; } = null!;

    public string Nombre { get; set; } = null!;

    public string? Version { get; set; }

    public virtual ICollection<Documento> Documentos { get; set; } = new List<Documento>();
}
