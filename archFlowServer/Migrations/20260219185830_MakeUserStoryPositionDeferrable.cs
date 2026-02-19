using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArchFlowServer.Migrations
{
    public partial class MakeUserStoryPositionDeferrable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Remove o índice unique padrão criado pelo EF (se existir)
            migrationBuilder.Sql(@"DROP INDEX IF EXISTS ""IX_user_stories_EpicId_Position"";");

            // Se a constraint já existir por algum motivo, remove antes (evita conflito)
            migrationBuilder.Sql(@"
                ALTER TABLE user_stories
                DROP CONSTRAINT IF EXISTS ""UQ_user_stories_EpicId_Position"";
            ");

            // Cria UNIQUE constraint DEFERRABLE
            migrationBuilder.Sql(@"
                ALTER TABLE user_stories
                ADD CONSTRAINT ""UQ_user_stories_EpicId_Position""
                UNIQUE (""EpicId"", ""Position"")
                DEFERRABLE INITIALLY DEFERRED;
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                ALTER TABLE user_stories
                DROP CONSTRAINT IF EXISTS ""UQ_user_stories_EpicId_Position"";
            ");

            // Volta ao padrão (índice unique normal)
            migrationBuilder.CreateIndex(
                name: "IX_user_stories_EpicId_Position",
                table: "user_stories",
                columns: new[] { "EpicId", "Position" },
                unique: true);
        }
    }
}
