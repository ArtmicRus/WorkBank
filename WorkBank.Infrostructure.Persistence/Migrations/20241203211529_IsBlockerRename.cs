using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorkBank.Infrostructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class IsBlockerRename : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsBlocked",
                table: "Persons",
                newName: "isBlocked");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "isBlocked",
                table: "Persons",
                newName: "IsBlocked");
        }
    }
}
