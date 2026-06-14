using Microsoft.EntityFrameworkCore.Migrations;
using System.IO;
#nullable disable

namespace NormaQ.Migrations
{
    /// <inheritdoc />
    public partial class TriggUpdateEstado : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            var sqlPath = Path.Combine(
                Directory.GetCurrentDirectory(),

                "sql",
                "trg_MutarEstadoVersion.sql"
            );

            var sql = File.ReadAllText(sqlPath);

            migrationBuilder.Sql(sql);

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.Sql(
           "DROP TRIGGER IF EXISTS trg_MutarEstadoVersion"
           );


        }
    }
}
