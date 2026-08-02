using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MesaSitec.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CompleteModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Solicitudes_TenantId",
                table: "Solicitudes");

            migrationBuilder.RenameColumn(
                name: "FechaAsignacion",
                table: "Solicitudes",
                newName: "MotivoResolucion");

            migrationBuilder.RenameColumn(
                name: "Activa",
                table: "Categorias",
                newName: "SlaHoras");

            migrationBuilder.AddColumn<string>(
                name: "Codigo",
                table: "Solicitudes",
                type: "TEXT",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaLimiteSla",
                table: "Solicitudes",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "MotivoCancelacion",
                table: "Solicitudes",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Activo",
                table: "Categorias",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_Email",
                table: "Usuarios",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Solicitudes_TenantId_Codigo",
                table: "Solicitudes",
                columns: new[] { "TenantId", "Codigo" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Usuarios_Email",
                table: "Usuarios");

            migrationBuilder.DropIndex(
                name: "IX_Solicitudes_TenantId_Codigo",
                table: "Solicitudes");

            migrationBuilder.DropColumn(
                name: "Codigo",
                table: "Solicitudes");

            migrationBuilder.DropColumn(
                name: "FechaLimiteSla",
                table: "Solicitudes");

            migrationBuilder.DropColumn(
                name: "MotivoCancelacion",
                table: "Solicitudes");

            migrationBuilder.DropColumn(
                name: "Activo",
                table: "Categorias");

            migrationBuilder.RenameColumn(
                name: "MotivoResolucion",
                table: "Solicitudes",
                newName: "FechaAsignacion");

            migrationBuilder.RenameColumn(
                name: "SlaHoras",
                table: "Categorias",
                newName: "Activa");

            migrationBuilder.CreateIndex(
                name: "IX_Solicitudes_TenantId",
                table: "Solicitudes",
                column: "TenantId");
        }
    }
}
