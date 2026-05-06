using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Desbravadores.Gestao.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SyncPostgresSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "QueryString",
                table: "ApiRequestLogs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RequestHeaders",
                table: "ApiRequestLogs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RequestBody",
                table: "ApiRequestLogs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResponseBody",
                table: "ApiRequestLogs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExceptionType",
                table: "ApiRequestLogs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExceptionMessage",
                table: "ApiRequestLogs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StackTrace",
                table: "ApiRequestLogs",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_UsuarioLogado",
                table: "Usuarios",
                column: "UsuarioLogado");

            migrationBuilder.AddForeignKey(
                name: "FK_Usuarios_Usuarios_UsuarioLogado",
                table: "Usuarios",
                column: "UsuarioLogado",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Usuarios_Usuarios_UsuarioLogado",
                table: "Usuarios");

            migrationBuilder.DropIndex(
                name: "IX_Usuarios_UsuarioLogado",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "QueryString",
                table: "ApiRequestLogs");

            migrationBuilder.DropColumn(
                name: "RequestHeaders",
                table: "ApiRequestLogs");

            migrationBuilder.DropColumn(
                name: "RequestBody",
                table: "ApiRequestLogs");

            migrationBuilder.DropColumn(
                name: "ResponseBody",
                table: "ApiRequestLogs");

            migrationBuilder.DropColumn(
                name: "ExceptionType",
                table: "ApiRequestLogs");

            migrationBuilder.DropColumn(
                name: "ExceptionMessage",
                table: "ApiRequestLogs");

            migrationBuilder.DropColumn(
                name: "StackTrace",
                table: "ApiRequestLogs");
        }
    }
}
