using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DotCast.Infrastructure.Persistence.EF.Identity.Migrations
{
    /// <inheritdoc />
    public partial class addedlibraries : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<List<string>>(
                name: "SharedLibraries",
                table: "AspNetUsers",
                type: "text[]",
                nullable: false);

            migrationBuilder.AddColumn<string>(
                name: "UsersLibraryName",
                table: "AspNetUsers",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SharedLibraries",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "UsersLibraryName",
                table: "AspNetUsers");
        }
    }
}
