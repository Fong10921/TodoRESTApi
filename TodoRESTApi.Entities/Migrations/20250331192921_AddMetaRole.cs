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

            migrationBuilder.CreateTable(
                name: "MetaRoles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    MetaRoleName = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MetaRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MetaRoleClaimsPivots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    RoleClaimId = table.Column<int>(type: "INTEGER", nullable: false),
                    MetaRoleId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MetaRoleClaimsPivots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MetaRoleClaimsPivots_AspNetRoleClaims_RoleClaimId",
                        column: x => x.RoleClaimId,
                        principalTable: "AspNetRoleClaims",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MetaRoleClaimsPivots_MetaRoles_MetaRoleId",
                        column: x => x.MetaRoleId,
                        principalTable: "MetaRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_MetaRoleId",
                table: "AspNetUsers",
                column: "MetaRoleId");

            migrationBuilder.CreateIndex(
                name: "IX_MetaRoleClaimsPivots_MetaRoleId",
                table: "MetaRoleClaimsPivots",
                column: "MetaRoleId");

            migrationBuilder.CreateIndex(
                name: "IX_MetaRoleClaimsPivots_RoleClaimId",
                table: "MetaRoleClaimsPivots",
                column: "RoleClaimId");

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
                name: "FK_AspNetUsers_MetaRoles_MetaRoleId",
                table: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "MetaRoleClaimsPivots");

            migrationBuilder.DropTable(
                name: "MetaRoles");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_MetaRoleId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "MetaRoleId",
                table: "AspNetUsers");
        }
    }
}
