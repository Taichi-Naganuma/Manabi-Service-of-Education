using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Manabi.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddUserActionLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserActionLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: true),
                    EventName = table.Column<string>(type: "text", nullable: false),
                    Page = table.Column<string>(type: "text", nullable: true),
                    TargetId = table.Column<string>(type: "text", nullable: true),
                    Meta = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserActionLogs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserActionLogs_UserId_EventName_CreatedAt",
                table: "UserActionLogs",
                columns: new[] { "UserId", "EventName", "CreatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserActionLogs");
        }
    }
}
