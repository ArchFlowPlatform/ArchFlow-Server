using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace archFlowServer.Migrations
{
    /// <inheritdoc />
    public partial class MakeEpicPositionUniqueDeferrable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // remove índice unique padrão do EF (ajuste o nome se necessário)
            migrationBuilder.Sql(@"DROP INDEX IF EXISTS ""IX_epics_ProductBacklogId_Position"";");

            // cria unique constraint DEFERRABLE (preferível a índice para isso)
            migrationBuilder.Sql(@"
        ALTER TABLE epics
        ADD CONSTRAINT ""UQ_epics_ProductBacklogId_Position""
        UNIQUE (""ProductBacklogId"", ""Position"")
        DEFERRABLE INITIALLY DEFERRED;
    ");
        }


        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
        ALTER TABLE epics
        DROP CONSTRAINT IF EXISTS ""UQ_epics_ProductBacklogId_Position"";
    ");

            migrationBuilder.CreateIndex(
                name: "IX_epics_ProductBacklogId_Position",
                table: "epics",
                columns: new[] { "ProductBacklogId", "Position" },
                unique: true);
        }

    }
}

