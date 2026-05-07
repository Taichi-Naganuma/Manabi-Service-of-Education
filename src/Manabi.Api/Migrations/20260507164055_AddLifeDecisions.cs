using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Manabi.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddLifeDecisions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LifeDecisions",
                table: "TeacherProfiles",
                type: "jsonb",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LifeDecisions",
                table: "TeacherProfiles");
        }
    }
}
