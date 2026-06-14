using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NormaQ.Migrations
{
    /// <inheritdoc />
    public partial class CorrectionDefaultValue : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<byte>(
                name: "version_mayor",
                table: "Versiones_Documento",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)0,
                oldClrType: typeof(byte),
                oldType: "tinyint",
                oldDefaultValue: (byte)1);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<byte>(
                name: "version_mayor",
                table: "Versiones_Documento",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)1,
                oldClrType: typeof(byte),
                oldType: "tinyint",
                oldDefaultValue: (byte)0);
        }
    }
}
