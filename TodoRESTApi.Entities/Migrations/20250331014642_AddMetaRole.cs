using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TodoRESTApi.Entities.Migrations
{
    /// <inheritdoc />
    public partial class AddMetaRole : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "MetaRoleId",
                table: "AspNetUsers",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "MetaRoleId",
                table: "AspNetRoleClaims",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "MetaRoles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    MetaRoleName = table.Column<string>(type: "TEXT", nullable: false),
                    RoleClaimsId = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MetaRoles", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_MetaRoleId",
                table: "AspNetUsers",
                column: "MetaRoleId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_MetaRoleId",
                table: "AspNetRoleClaims",
                column: "MetaRoleId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetRoleClaims_MetaRoles_MetaRoleId",
                table: "AspNetRoleClaims",
                column: "MetaRoleId",
                principalTable: "MetaRoles",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_MetaRoles_MetaRoleId",
                table: "AspNetUsers",
                column: "MetaRoleId",
                principalTable: "MetaRoles",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetRoleClaims_MetaRoles_MetaRoleId",
                table: "AspNetRoleClaims");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_MetaRoles_MetaRoleId",
                table: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "MetaRoles");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_MetaRoleId",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetRoleClaims_MetaRoleId",
                table: "AspNetRoleClaims");

            migrationBuilder.DropColumn(
                name: "MetaRoleId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "MetaRoleId",
                table: "AspNetRoleClaims");
        }
    }
}
