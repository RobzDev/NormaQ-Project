using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NormaQ.Migrations
{
    /// <inheritdoc />
    public partial class AddVw_DocumentosAprobados : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            var sqlPath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "sql",
                "vw_Documentos_Aprobados.sql"
            );

            var sql = File.ReadAllText(sqlPath);

            migrationBuilder.Sql(sql);

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP VIEW IF EXISTS vw_Documentos_Aprobados");

        }
    }
}
