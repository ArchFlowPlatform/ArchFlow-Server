using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArchFlowServer.Migrations
{
    public partial class MakeUserStoryPositionDeferrable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Remove o índice unique padrão criado pelo EF (se existir)
            migrationBuilder.Sql(@"DROP INDEX IF EXISTS ""IX_user_stories_EpicId_BacklogPosition"";");

            // Se a constraint já existir por algum motivo, remove antes (evita conflito)
            migrationBuilder.Sql(@"
                ALTER TABLE user_stories
                DROP CONSTRAINT IF EXISTS ""UQ_user_stories_EpicId_BacklogPosition"";
            ");

            // Cria UNIQUE constraint DEFERRABLE
            migrationBuilder.Sql(@"
                ALTER TABLE user_stories
                ADD CONSTRAINT ""UQ_user_stories_EpicId_BacklogPosition""
                UNIQUE (""EpicId"", ""BacklogPosition"")
                DEFERRABLE INITIALLY DEFERRED;
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                ALTER TABLE user_stories
                DROP CONSTRAINT IF EXISTS ""UQ_user_stories_EpicId_BacklogPosition"";
            ");

            // Volta ao padrão (índice unique normal)
            migrationBuilder.CreateIndex(
                name: "IX_user_stories_EpicId_BacklogPosition",
                table: "user_stories",
                columns: new[] { "EpicId", "BacklogPosition" },
                unique: true);
        }
    }
}