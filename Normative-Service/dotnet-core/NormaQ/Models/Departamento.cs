using System;
using System.Collections.Generic;

namespace NormaQ.Models;

public partial class Departamento
{
    public int Id { get; set; }

    public string Nombre { get; set; } = null!;

    public int CompaniaId { get; set; }

    public bool Activo { get; set; }

    public virtual Compania Compania { get; set; } = null!;

    public virtual ICollection<Documento> Documentos { get; set; } = new List<Documento>();

    public virtual ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();

    public virtual ICollection<UsuariosRole> UsuariosRoles { get; set; } = new List<UsuariosRole>();
}
