using System;
using System.Collections.Generic;

namespace NormaQ.Models;

public partial class Compania
{
    public int Id { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Rfc { get; set; }

    public string? Direccion { get; set; }

    public bool Activo { get; set; }

    public DateTime CreadoEn { get; set; }

    public virtual ICollection<Departamento> Departamentos { get; set; } = new List<Departamento>();
}
