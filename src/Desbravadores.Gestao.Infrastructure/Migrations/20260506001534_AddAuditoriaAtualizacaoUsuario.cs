using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Desbravadores.Gestao.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditoriaAtualizacaoUsuario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DataAtualizacao",
                table: "Usuarios",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IpUsuarioLogado",
                table: "Usuarios",
                type: "nvarchar(45)",
                maxLength: 45,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UsuarioLogado",
                table: "Usuarios",
                type: "int",
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
                name: "DataAtualizacao",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "IpUsuarioLogado",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "UsuarioLogado",
                table: "Usuarios");
        }
    }
}
