using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using NormaQ.Models;

namespace NormaQ.Data;

public partial class AppDbContext : Microsoft.EntityFrameworkCore.DbContext
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Compania> Companias { get; set; }

    public virtual DbSet<Departamento> Departamentos { get; set; }

    public virtual DbSet<Documento> Documentos { get; set; }

    public virtual DbSet<FlujosAprobacion> FlujosAprobacions { get; set; }

    public virtual DbSet<NivelesDocumento> NivelesDocumentos { get; set; }

    public virtual DbSet<Norma> Normas { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<SecuenciaFirma> SecuenciaFirmas { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    public virtual DbSet<UsuariosRole> UsuariosRoles { get; set; }

    public virtual DbSet<VersionesDocumento> VersionesDocumentos { get; set; }


    public DbSet<DocumentoAprobadoView> DocumentosAprobados { get; set; }



    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Compania>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Compania__3213E83F952175FC");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.CreadoEn)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("creado_en");
            entity.Property(e => e.Direccion)
                .HasMaxLength(255)
                .HasColumnName("direccion");
            entity.Property(e => e.Nombre)
                .HasMaxLength(150)
                .HasColumnName("nombre");
            entity.Property(e => e.Rfc)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("rfc");
        });

        modelBuilder.Entity<Departamento>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Departam__3213E83F82742017");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.CompaniaId).HasColumnName("compania_id");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .HasColumnName("nombre");

            entity.HasOne(d => d.Compania).WithMany(p => p.Departamentos)
                .HasForeignKey(d => d.CompaniaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Deptos_Companias");
        });

        modelBuilder.Entity<Documento>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Document__3213E83FCE9B336E");

            entity.HasIndex(e => e.DepartamentoId, "IX_Documentos_Depto");

            entity.HasIndex(e => e.Codigo, "UQ_Documentos_Codigo").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Codigo)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("codigo");
            entity.Property(e => e.CreadoEn)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("creado_en");
            entity.Property(e => e.CreadoPor).HasColumnName("creado_por");
            entity.Property(e => e.DepartamentoId).HasColumnName("departamento_id");
            entity.Property(e => e.NivelId).HasColumnName("nivel_id");
            entity.Property(e => e.Nombre)
                .HasMaxLength(255)
                .HasColumnName("nombre");
            entity.Property(e => e.NormaId).HasColumnName("norma_id");

            entity.HasOne(d => d.CreadoPorNavigation).WithMany(p => p.Documentos)
                .HasForeignKey(d => d.CreadoPor)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Docs_Creador");

            entity.HasOne(d => d.Departamento).WithMany(p => p.Documentos)
                .HasForeignKey(d => d.DepartamentoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Docs_Depto");

            entity.HasOne(d => d.Nivel).WithMany(p => p.Documentos)
                .HasForeignKey(d => d.NivelId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Docs_Nivel");

            entity.HasOne(d => d.Norma).WithMany(p => p.Documentos)
                .HasForeignKey(d => d.NormaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Docs_Norma");
        });

        modelBuilder.Entity<FlujosAprobacion>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Flujos_A__3213E83FC1B62FB2");

            entity.ToTable("Flujos_Aprobacion", t =>
            {
                t.HasCheckConstraint(
                    "CHK_Flujos_Estado",
                "estado_firma IN ('Pendiente', 'Aprobado', 'Rechazado', 'Cancelado')");

                t.HasTrigger("trg_MutarEstadoVersion");
            });

            entity.HasIndex(e => new { e.UsuarioId, e.EstadoFirma }, "IX_Flujos_UsuarioEstado");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Comentarios)
                .HasMaxLength(500)
                .HasColumnName("comentarios");
            entity.Property(e => e.EstadoFirma)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("Pendiente")
                .HasColumnName("estado_firma");
            entity.Property(e => e.FechaFirma)
                .HasColumnType("datetime")
                .HasColumnName("fecha_firma");
            entity.Property(e => e.Orden).HasColumnName("orden");
            entity.Property(e => e.TipoFirma)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("tipo_firma");
            entity.Property(e => e.UsuarioId).HasColumnName("usuario_id");
            entity.Property(e => e.VersionId).HasColumnName("version_id");

            entity.HasOne(d => d.Usuario).WithMany(p => p.FlujosAprobacions)
                .HasForeignKey(d => d.UsuarioId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Flujos_Usuario");

            entity.HasOne(d => d.Version).WithMany(p => p.FlujosAprobacions)
                .HasForeignKey(d => d.VersionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Flujos_Version");
        });

        modelBuilder.Entity<NivelesDocumento>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Niveles___3213E83F2B73C355");

            entity.ToTable("Niveles_Documento");

            entity.HasIndex(e => e.Numero, "UQ_Niveles_Numero").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Nombre)
                .HasMaxLength(80)
                .HasColumnName("nombre");
            entity.Property(e => e.Numero).HasColumnName("numero");
        });

        modelBuilder.Entity<Norma>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Normas__3213E83F4A76CF67");

            entity.HasIndex(e => e.Codigo, "UQ_Normas_Codigo").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Codigo)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("codigo");
            entity.Property(e => e.Nombre)
                .HasMaxLength(150)
                .HasColumnName("nombre");
            entity.Property(e => e.Version)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("version");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Roles__3213E83FAFCD81BD");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(255)
                .HasColumnName("descripcion");
            entity.Property(e => e.Nombre)
                .HasMaxLength(80)
                .HasColumnName("nombre");
        });

        modelBuilder.Entity<SecuenciaFirma>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Secuenci__3213E83FD0E8379F");

            entity.ToTable("Secuencia_Firma");

            entity.HasIndex(e => new { e.DocumentoId, e.Orden }, "UQ_SecFirma_Orden").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.DocumentoId).HasColumnName("documento_id");
            entity.Property(e => e.Orden).HasColumnName("orden");
            entity.Property(e => e.RolId).HasColumnName("rol_id");
            entity.Property(e => e.TipoFirma)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("tipo_firma");

            entity.HasOne(d => d.Documento).WithMany(p => p.SecuenciaFirmas)
                .HasForeignKey(d => d.DocumentoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SecFirma_Doc");

            entity.HasOne(d => d.Rol).WithMany(p => p.SecuenciaFirmas)
                .HasForeignKey(d => d.RolId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SecFirma_Rol");
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Usuarios__3213E83F893CFD1F");

            entity.HasIndex(e => e.Email, "UQ_Usuarios_Email").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.CreadoEn)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("creado_en");
            entity.Property(e => e.DepartamentoId).HasColumnName("departamento_id");
            entity.Property(e => e.Email)
                .HasMaxLength(150)
                .IsUnicode(false)
                .HasColumnName("email");
            entity.Property(e => e.Nombre)
                .HasMaxLength(150)
                .HasColumnName("nombre");
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("password_hash");

            entity.HasOne(d => d.Departamento).WithMany(p => p.Usuarios)
                .HasForeignKey(d => d.DepartamentoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Usuarios_Deptos");
        });

        modelBuilder.Entity<UsuariosRole>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Usuarios__3213E83F11B0C0F4");

            entity.ToTable("Usuarios_Roles");

            entity.HasIndex(e => new { e.UsuarioId, e.RolId, e.DepartamentoId }, "UQ_UsuariosRoles").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.DepartamentoId).HasColumnName("departamento_id");
            entity.Property(e => e.RolId).HasColumnName("rol_id");
            entity.Property(e => e.UsuarioId).HasColumnName("usuario_id");

            entity.HasOne(d => d.Departamento).WithMany(p => p.UsuariosRoles)
                .HasForeignKey(d => d.DepartamentoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UsuRoles_Depto");

            entity.HasOne(d => d.Rol).WithMany(p => p.UsuariosRoles)
                .HasForeignKey(d => d.RolId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UsuRoles_Rol");

            entity.HasOne(d => d.Usuario).WithMany(p => p.UsuariosRoles)
                .HasForeignKey(d => d.UsuarioId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UsuRoles_Usuario");
        });

        modelBuilder.Entity<VersionesDocumento>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Versione__3213E83FE6E0D886");

            entity.ToTable("Versiones_Documento");

            entity.HasIndex(e => e.Estado, "IX_Versiones_Estado");

            entity.HasIndex(e => new { e.DocumentoId, e.VersionMayor, e.VersionMenor }, "UQ_Versiones_Numero").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreadoPor).HasColumnName("creado_por");
            entity.Property(e => e.DocumentoId).HasColumnName("documento_id");
            entity.Property(e => e.Estado)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("Borrador")
                .HasColumnName("estado");
            entity.Property(e => e.FechaAprobacion)
                .HasColumnType("datetime")
                .HasColumnName("fecha_aprobacion");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.FechaObsolescencia)
                .HasColumnType("datetime")
                .HasColumnName("fecha_obsolescencia");
            entity.Property(e => e.MinioIdentifier)
                .HasMaxLength(500)
                .IsUnicode(false)
                .HasColumnName("minio_identifier");
            entity.Property(e => e.VersionMayor)
                .HasDefaultValue((byte)1)
                .HasColumnName("version_mayor");
            entity.Property(e => e.VersionMenor).HasColumnName("version_menor");

            entity.HasOne(d => d.CreadoPorNavigation).WithMany(p => p.VersionesDocumentos)
                .HasForeignKey(d => d.CreadoPor)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Ver_Creador");

            entity.HasOne(d => d.Documento).WithMany(p => p.VersionesDocumentos)
                .HasForeignKey(d => d.DocumentoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Ver_Documento");



        });



        modelBuilder.Entity<DocumentoAprobadoView>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("vw_Documentos_Aprobados");

                entity.Property(e => e.VersionId)
                    .HasColumnName("version_id");

                entity.Property(e => e.DocumentoId)
                    .HasColumnName("documento_id");

                entity.Property(e => e.CodigoDocumento)
                    .HasColumnName("codigo_documento");

                entity.Property(e => e.NombreDocumento)
                    .HasColumnName("nombre_documento");

                entity.Property(e => e.Nivel)
                    .HasColumnName("nivel");

                entity.Property(e => e.NormaCodigo)
                    .HasColumnName("norma_codigo");

                entity.Property(e => e.NormaNombre)
                    .HasColumnName("norma_nombre");

                entity.Property(e => e.Departamento)
                    .HasColumnName("departamento");

                entity.Property(e => e.Version)
                    .HasColumnName("version");

                entity.Property(e => e.Owner)
                    .HasColumnName("owner");

                entity.Property(e => e.ApprovedBy)
                    .HasColumnName("approved_by");

                entity.Property(e => e.ApprovedAt)
                    .HasColumnName("approved_at");

                entity.Property(e => e.CreatedAt)
                    .HasColumnName("created_at");

                entity.Property(e => e.StoragePath)
                    .HasColumnName("storage_path");
            });




        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
