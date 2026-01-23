using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace archFlowServer.Migrations
{
    public partial class MakeUserStoryPositionDeferrable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Remove o índice unique padrão criado pelo EF (ajuste o nome se estiver diferente)
            migrationBuilder.Sql(@"DROP INDEX IF EXISTS ""IX_user_stories_EpicId_Position"";");

            // Cria UNIQUE constraint DEFERRABLE (PostgreSQL)
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

            // Recria o índice unique padrão para voltar ao estado anterior
            migrationBuilder.CreateIndex(
                name: "IX_user_stories_EpicId_Position",
                table: "user_stories",
                columns: new[] { "EpicId", "Position" },
                unique: true);
        }
    }
}
