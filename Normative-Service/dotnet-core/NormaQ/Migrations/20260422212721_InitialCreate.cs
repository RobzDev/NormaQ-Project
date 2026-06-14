using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NormaQ.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Companias",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    nombre = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    rfc = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true),
                    direccion = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    creado_en = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Compania__3213E83F952175FC", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Niveles_Documento",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    numero = table.Column<byte>(type: "tinyint", nullable: false),
                    nombre = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Niveles___3213E83F2B73C355", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Normas",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    codigo = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: false),
                    nombre = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    version = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Normas__3213E83F4A76CF67", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    nombre = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    descripcion = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Roles__3213E83FAFCD81BD", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Departamentos",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    compania_id = table.Column<int>(type: "int", nullable: false),
                    activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Departam__3213E83F82742017", x => x.id);
                    table.ForeignKey(
                        name: "FK_Deptos_Companias",
                        column: x => x.compania_id,
                        principalTable: "Companias",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    nombre = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    email = table.Column<string>(type: "varchar(150)", unicode: false, maxLength: 150, nullable: false),
                    password_hash = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: false),
                    departamento_id = table.Column<int>(type: "int", nullable: false),
                    activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    creado_en = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Usuarios__3213E83F893CFD1F", x => x.id);
                    table.ForeignKey(
                        name: "FK_Usuarios_Deptos",
                        column: x => x.departamento_id,
                        principalTable: "Departamentos",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "Documentos",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    codigo = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: false),
                    nombre = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    nivel_id = table.Column<int>(type: "int", nullable: false),
                    norma_id = table.Column<int>(type: "int", nullable: false),
                    departamento_id = table.Column<int>(type: "int", nullable: false),
                    creado_por = table.Column<int>(type: "int", nullable: false),
                    creado_en = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Document__3213E83FCE9B336E", x => x.id);
                    table.ForeignKey(
                        name: "FK_Docs_Creador",
                        column: x => x.creado_por,
                        principalTable: "Usuarios",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_Docs_Depto",
                        column: x => x.departamento_id,
                        principalTable: "Departamentos",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_Docs_Nivel",
                        column: x => x.nivel_id,
                        principalTable: "Niveles_Documento",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_Docs_Norma",
                        column: x => x.norma_id,
                        principalTable: "Normas",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "Usuarios_Roles",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    usuario_id = table.Column<int>(type: "int", nullable: false),
                    rol_id = table.Column<int>(type: "int", nullable: false),
                    departamento_id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Usuarios__3213E83F11B0C0F4", x => x.id);
                    table.ForeignKey(
                        name: "FK_UsuRoles_Depto",
                        column: x => x.departamento_id,
                        principalTable: "Departamentos",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_UsuRoles_Rol",
                        column: x => x.rol_id,
                        principalTable: "Roles",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_UsuRoles_Usuario",
                        column: x => x.usuario_id,
                        principalTable: "Usuarios",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "Secuencia_Firma",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    documento_id = table.Column<int>(type: "int", nullable: false),
                    rol_id = table.Column<int>(type: "int", nullable: false),
                    tipo_firma = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    orden = table.Column<byte>(type: "tinyint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Secuenci__3213E83FD0E8379F", x => x.id);
                    table.ForeignKey(
                        name: "FK_SecFirma_Doc",
                        column: x => x.documento_id,
                        principalTable: "Documentos",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_SecFirma_Rol",
                        column: x => x.rol_id,
                        principalTable: "Roles",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "Versiones_Documento",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    documento_id = table.Column<int>(type: "int", nullable: false),
                    version_mayor = table.Column<byte>(type: "tinyint", nullable: false, defaultValue: (byte)0),
                    version_menor = table.Column<byte>(type: "tinyint", nullable: false),
                    estado = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false, defaultValue: "Borrador"),
                    minio_identifier = table.Column<string>(type: "varchar(500)", unicode: false, maxLength: 500, nullable: false),
                    creado_por = table.Column<int>(type: "int", nullable: false),
                    fecha_creacion = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())"),
                    fecha_aprobacion = table.Column<DateTime>(type: "datetime", nullable: true),
                    fecha_obsolescencia = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Versione__3213E83FE6E0D886", x => x.id);
                    table.ForeignKey(
                        name: "FK_Ver_Creador",
                        column: x => x.creado_por,
                        principalTable: "Usuarios",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_Ver_Documento",
                        column: x => x.documento_id,
                        principalTable: "Documentos",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "Flujos_Aprobacion",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    version_id = table.Column<int>(type: "int", nullable: false),
                    usuario_id = table.Column<int>(type: "int", nullable: false),
                    tipo_firma = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    orden = table.Column<byte>(type: "tinyint", nullable: false),
                    estado_firma = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false, defaultValue: "Pendiente"),
                    comentarios = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    fecha_firma = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Flujos_A__3213E83FC1B62FB2", x => x.id);
                    table.ForeignKey(
                        name: "FK_Flujos_Usuario",
                        column: x => x.usuario_id,
                        principalTable: "Usuarios",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_Flujos_Version",
                        column: x => x.version_id,
                        principalTable: "Versiones_Documento",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Departamentos_compania_id",
                table: "Departamentos",
                column: "compania_id");

            migrationBuilder.CreateIndex(
                name: "IX_Documentos_creado_por",
                table: "Documentos",
                column: "creado_por");

            migrationBuilder.CreateIndex(
                name: "IX_Documentos_Depto",
                table: "Documentos",
                column: "departamento_id");

            migrationBuilder.CreateIndex(
                name: "IX_Documentos_nivel_id",
                table: "Documentos",
                column: "nivel_id");

            migrationBuilder.CreateIndex(
                name: "IX_Documentos_norma_id",
                table: "Documentos",
                column: "norma_id");

            migrationBuilder.CreateIndex(
                name: "UQ_Documentos_Codigo",
                table: "Documentos",
                column: "codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Flujos_Aprobacion_version_id",
                table: "Flujos_Aprobacion",
                column: "version_id");

            migrationBuilder.CreateIndex(
                name: "IX_Flujos_UsuarioEstado",
                table: "Flujos_Aprobacion",
                columns: new[] { "usuario_id", "estado_firma" });

            migrationBuilder.CreateIndex(
                name: "UQ_Niveles_Numero",
                table: "Niveles_Documento",
                column: "numero",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_Normas_Codigo",
                table: "Normas",
                column: "codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Secuencia_Firma_rol_id",
                table: "Secuencia_Firma",
                column: "rol_id");

            migrationBuilder.CreateIndex(
                name: "UQ_SecFirma_Orden",
                table: "Secuencia_Firma",
                columns: new[] { "documento_id", "orden" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_departamento_id",
                table: "Usuarios",
                column: "departamento_id");

            migrationBuilder.CreateIndex(
                name: "UQ_Usuarios_Email",
                table: "Usuarios",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_Roles_departamento_id",
                table: "Usuarios_Roles",
                column: "departamento_id");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_Roles_rol_id",
                table: "Usuarios_Roles",
                column: "rol_id");

            migrationBuilder.CreateIndex(
                name: "UQ_UsuariosRoles",
                table: "Usuarios_Roles",
                columns: new[] { "usuario_id", "rol_id", "departamento_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Versiones_Documento_creado_por",
                table: "Versiones_Documento",
                column: "creado_por");

            migrationBuilder.CreateIndex(
                name: "IX_Versiones_Estado",
                table: "Versiones_Documento",
                column: "estado");

            migrationBuilder.CreateIndex(
                name: "UQ_Versiones_Numero",
                table: "Versiones_Documento",
                columns: new[] { "documento_id", "version_mayor", "version_menor" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Flujos_Aprobacion");

            migrationBuilder.DropTable(
                name: "Secuencia_Firma");

            migrationBuilder.DropTable(
                name: "Usuarios_Roles");

            migrationBuilder.DropTable(
                name: "Versiones_Documento");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "Documentos");

            migrationBuilder.DropTable(
                name: "Usuarios");

            migrationBuilder.DropTable(
                name: "Niveles_Documento");

            migrationBuilder.DropTable(
                name: "Normas");

            migrationBuilder.DropTable(
                name: "Departamentos");

            migrationBuilder.DropTable(
                name: "Companias");
        }
    }
}
