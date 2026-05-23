using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NormaQ.Migrations
{
    /// <inheritdoc />
    public partial class SolicitudesSnakeCase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Solicitudes_Departamentos",
                table: "SolicitudesRegistro");

            migrationBuilder.DropForeignKey(
                name: "FK_Solicitudes_Roles",
                table: "SolicitudesRegistro");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SolicitudesRegistro",
                table: "SolicitudesRegistro");

            migrationBuilder.RenameTable(
                name: "SolicitudesRegistro",
                newName: "Solicitudes_Registro");

            migrationBuilder.RenameIndex(
                name: "UQ_SolicitudesRegistro_Email",
                table: "Solicitudes_Registro",
                newName: "UQ_Solicitudes_Registro_Email");

            migrationBuilder.RenameIndex(
                name: "IX_SolicitudesRegistro_rol_id",
                table: "Solicitudes_Registro",
                newName: "IX_Solicitudes_Registro_rol_id");

            migrationBuilder.RenameIndex(
                name: "IX_SolicitudesRegistro_departamento_id",
                table: "Solicitudes_Registro",
                newName: "IX_Solicitudes_Registro_departamento_id");

            migrationBuilder.AlterColumn<string>(
                name: "password_hash",
                table: "Solicitudes_Registro",
                type: "varchar(255)",
                unicode: false,
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "fecha_solicitud",
                table: "Solicitudes_Registro",
                type: "datetime",
                nullable: false,
                defaultValueSql: "(getutcdate())",
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldDefaultValueSql: "(getdate())");

            migrationBuilder.AlterColumn<string>(
                name: "email",
                table: "Solicitudes_Registro",
                type: "varchar(150)",
                unicode: false,
                maxLength: 150,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(150)",
                oldMaxLength: 150);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Solicitudes_Registro",
                table: "Solicitudes_Registro",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_Solicitudes_Departamentos",
                table: "Solicitudes_Registro",
                column: "departamento_id",
                principalTable: "Departamentos",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_Solicitudes_Roles",
                table: "Solicitudes_Registro",
                column: "rol_id",
                principalTable: "Roles",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Solicitudes_Departamentos",
                table: "Solicitudes_Registro");

            migrationBuilder.DropForeignKey(
                name: "FK_Solicitudes_Roles",
                table: "Solicitudes_Registro");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Solicitudes_Registro",
                table: "Solicitudes_Registro");

            migrationBuilder.RenameTable(
                name: "Solicitudes_Registro",
                newName: "SolicitudesRegistro");

            migrationBuilder.RenameIndex(
                name: "UQ_Solicitudes_Registro_Email",
                table: "SolicitudesRegistro",
                newName: "UQ_SolicitudesRegistro_Email");

            migrationBuilder.RenameIndex(
                name: "IX_Solicitudes_Registro_rol_id",
                table: "SolicitudesRegistro",
                newName: "IX_SolicitudesRegistro_rol_id");

            migrationBuilder.RenameIndex(
                name: "IX_Solicitudes_Registro_departamento_id",
                table: "SolicitudesRegistro",
                newName: "IX_SolicitudesRegistro_departamento_id");

            migrationBuilder.AlterColumn<string>(
                name: "password_hash",
                table: "SolicitudesRegistro",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(255)",
                oldUnicode: false,
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<DateTime>(
                name: "fecha_solicitud",
                table: "SolicitudesRegistro",
                type: "datetime",
                nullable: false,
                defaultValueSql: "(getdate())",
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldDefaultValueSql: "(getutcdate())");

            migrationBuilder.AlterColumn<string>(
                name: "email",
                table: "SolicitudesRegistro",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(150)",
                oldUnicode: false,
                oldMaxLength: 150);

            migrationBuilder.AddPrimaryKey(
                name: "PK_SolicitudesRegistro",
                table: "SolicitudesRegistro",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_Solicitudes_Departamentos",
                table: "SolicitudesRegistro",
                column: "departamento_id",
                principalTable: "Departamentos",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Solicitudes_Roles",
                table: "SolicitudesRegistro",
                column: "rol_id",
                principalTable: "Roles",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
