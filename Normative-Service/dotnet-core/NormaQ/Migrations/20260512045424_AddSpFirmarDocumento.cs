using Microsoft.EntityFrameworkCore.Migrations;
using System.IO;

#nullable disable

namespace NormaQ.Migrations
{
    /// <inheritdoc />
    public partial class AddSpFirmarDocumento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sqlPath = Path.Combine(
                Directory.GetCurrentDirectory(),
             
                "sql",
                "SP_Generar_Flujos_Aprobacion.sql"
            );

            var sql = File.ReadAllText(sqlPath);

            migrationBuilder.Sql(sql);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.Sql(
            "DROP PROCEDURE IF EXISTS SP_Generar_Flujos_Aprobacion"
            );

        }
    }
}
