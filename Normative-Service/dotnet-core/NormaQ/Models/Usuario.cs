using System;
using System.Collections.Generic;

namespace NormaQ.Models;

public partial class Usuario
{
    public int Id { get; set; }

    public string Nombre { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public int DepartamentoId { get; set; }

    public bool Activo { get; set; }

    public DateTime CreadoEn { get; set; }

    public virtual Departamento Departamento { get; set; } = null!;

    public virtual ICollection<Documento> Documentos { get; set; } = new List<Documento>();

    public virtual ICollection<FlujosAprobacion> FlujosAprobacions { get; set; } = new List<FlujosAprobacion>();

    public virtual ICollection<UsuariosRole> UsuariosRoles { get; set; } = new List<UsuariosRole>();

    public virtual ICollection<VersionesDocumento> VersionesDocumentos { get; set; } = new List<VersionesDocumento>();
}
