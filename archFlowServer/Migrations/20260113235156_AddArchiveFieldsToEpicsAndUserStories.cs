using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace archFlowServer.Migrations
{
    /// <inheritdoc />
    public partial class AddArchiveFieldsToEpicsAndUserStories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ArchivedAt",
                table: "user_stories",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsArchived",
                table: "user_stories",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "ArchivedAt",
                table: "epics",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsArchived",
                table: "epics",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_user_stories_EpicId_IsArchived",
                table: "user_stories",
                columns: new[] { "EpicId", "IsArchived" });

            migrationBuilder.CreateIndex(
                name: "IX_epics_ProductBacklogId_IsArchived",
                table: "epics",
                columns: new[] { "ProductBacklogId", "IsArchived" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_user_stories_EpicId_IsArchived",
                table: "user_stories");

            migrationBuilder.DropIndex(
                name: "IX_epics_ProductBacklogId_IsArchived",
                table: "epics");

            migrationBuilder.DropColumn(
                name: "ArchivedAt",
                table: "user_stories");

            migrationBuilder.DropColumn(
                name: "IsArchived",
                table: "user_stories");

            migrationBuilder.DropColumn(
                name: "ArchivedAt",
                table: "epics");

            migrationBuilder.DropColumn(
                name: "IsArchived",
                table: "epics");
        }
    }
}

