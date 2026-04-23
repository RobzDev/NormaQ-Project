using System;
using System.Collections.Generic;

namespace NormaQ.Models;

public partial class VersionesDocumento
{
    public int Id { get; set; }

    public int DocumentoId { get; set; }

    public byte VersionMayor { get; set; }

    public byte VersionMenor { get; set; }

    public string Estado { get; set; } = null!;

    public string MinioIdentifier { get; set; } = null!;

    public int CreadoPor { get; set; }

    public DateTime FechaCreacion { get; set; }

    public DateTime? FechaAprobacion { get; set; }

    public DateTime? FechaObsolescencia { get; set; }

    public virtual Usuario CreadoPorNavigation { get; set; } = null!;

    public virtual Documento Documento { get; set; } = null!;

    public virtual ICollection<FlujosAprobacion> FlujosAprobacions { get; set; } = new List<FlujosAprobacion>();
}
