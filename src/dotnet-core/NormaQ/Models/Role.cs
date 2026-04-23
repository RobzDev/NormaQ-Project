using System;
using System.Collections.Generic;

namespace NormaQ.Models;

public partial class Role
{
    public int Id { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Descripcion { get; set; }

    public virtual ICollection<SecuenciaFirma> SecuenciaFirmas { get; set; } = new List<SecuenciaFirma>();

    public virtual ICollection<UsuariosRole> UsuariosRoles { get; set; } = new List<UsuariosRole>();
}
