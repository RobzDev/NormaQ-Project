using System;
using System.Collections.Generic;

namespace NormaQ.Models;

public partial class NivelesDocumento
{
    public int Id { get; set; }

    public byte Numero { get; set; }

    public string Nombre { get; set; } = null!;

    public virtual ICollection<Documento> Documentos { get; set; } = new List<Documento>();
}
