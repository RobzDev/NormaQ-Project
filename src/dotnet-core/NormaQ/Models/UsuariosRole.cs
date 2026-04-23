using System;
using System.Collections.Generic;

namespace NormaQ.Models;

public partial class UsuariosRole
{
    public int Id { get; set; }

    public int UsuarioId { get; set; }

    public int RolId { get; set; }

    public int DepartamentoId { get; set; }

    public virtual Departamento Departamento { get; set; } = null!;

    public virtual Role Rol { get; set; } = null!;

    public virtual Usuario Usuario { get; set; } = null!;
}
