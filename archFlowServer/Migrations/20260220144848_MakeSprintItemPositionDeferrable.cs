using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArchFlowServer.Migrations
{
    public partial class MakeSprintItemPositionDeferrable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP INDEX IF EXISTS ""IX_sprint_items_SprintId_Position"";");

            migrationBuilder.Sql(@"
                ALTER TABLE sprint_items
                DROP CONSTRAINT IF EXISTS ""UQ_sprint_items_SprintId_Position"";
            ");

            migrationBuilder.Sql(@"
                ALTER TABLE sprint_items
                ADD CONSTRAINT ""UQ_sprint_items_SprintId_Position""
                UNIQUE (""SprintId"", ""Position"")
                DEFERRABLE INITIALLY DEFERRED;
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                ALTER TABLE sprint_items
                DROP CONSTRAINT IF EXISTS ""UQ_sprint_items_SprintId_Position"";
            ");

            migrationBuilder.CreateIndex(
                name: "IX_sprint_items_SprintId_Position",
                table: "sprint_items",
                columns: new[] { "SprintId", "Position" },
                unique: true);
        }
    }
}