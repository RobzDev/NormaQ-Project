using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NormaQ.Migrations
{
    /// <inheritdoc />
    public partial class AddEstadoFirmaCheckConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddCheckConstraint(
                name: "CHK_Flujos_Estado",
                table: "Flujos_Aprobacion",
                sql: "estado_firma IN ('Pendiente', 'Aprobado', 'Rechazado', 'Cancelado')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CHK_Flujos_Estado",
                table: "Flujos_Aprobacion");
        }
    }
}
