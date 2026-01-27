using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArchFlowServer.Migrations
{
    public partial class MakeEpicPositionUniqueDeferrable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Remove o índice unique padrão criado pelo EF (se existir)
            migrationBuilder.Sql(@"DROP INDEX IF EXISTS ""IX_epics_ProductBacklogId_Position"";");

            // Se a constraint já existir por algum motivo, remove antes (evita conflito)
            migrationBuilder.Sql(@"
                ALTER TABLE epics
                DROP CONSTRAINT IF EXISTS ""UQ_epics_ProductBacklogId_Position"";
            ");

            // Cria UNIQUE constraint DEFERRABLE
            migrationBuilder.Sql(@"
                ALTER TABLE epics
                ADD CONSTRAINT ""UQ_epics_ProductBacklogId_Position""
                UNIQUE (""ProductBacklogId"", ""Position"")
                DEFERRABLE INITIALLY DEFERRED;
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                ALTER TABLE epics
                DROP CONSTRAINT IF EXISTS ""UQ_epics_ProductBacklogId_Position"";
            ");

            // Volta ao padrão (índice unique normal)
            migrationBuilder.CreateIndex(
                name: "IX_epics_ProductBacklogId_Position",
                table: "epics",
                columns: new[] { "ProductBacklogId", "Position" },
                unique: true);
        }
    }
}

