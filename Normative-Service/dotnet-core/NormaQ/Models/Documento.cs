using System;
using System.Collections.Generic;

namespace NormaQ.Models;

public partial class Documento
{
    public int Id { get; set; }

    public string Codigo { get; set; } = null!;

    public string Nombre { get; set; } = null!;

    public int NivelId { get; set; }

    public int NormaId { get; set; }

    public int DepartamentoId { get; set; }

    public int CreadoPor { get; set; }

    public DateTime CreadoEn { get; set; }

    public virtual Usuario CreadoPorNavigation { get; set; } = null!;

    public virtual Departamento Departamento { get; set; } = null!;

    public virtual NivelesDocumento Nivel { get; set; } = null!;

    public virtual Norma Norma { get; set; } = null!;

    public virtual ICollection<SecuenciaFirma> SecuenciaFirmas { get; set; } = new List<SecuenciaFirma>();

    public virtual ICollection<VersionesDocumento> VersionesDocumentos { get; set; } = new List<VersionesDocumento>();
}
