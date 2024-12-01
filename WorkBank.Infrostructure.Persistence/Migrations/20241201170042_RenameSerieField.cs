using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorkBank.Infrostructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RenameSerieField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Sirie",
                table: "Passports",
                newName: "Serie");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Serie",
                table: "Passports",
                newName: "Sirie");
        }
    }
}
