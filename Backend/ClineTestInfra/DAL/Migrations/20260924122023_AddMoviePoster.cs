using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClineTestInfra.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddMoviePoster : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder
                .AddColumn<string>(name: "PosterUrl", table: "movies", type: "varchar(500)", maxLength: 500, nullable: false, defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "movies",
                keyColumn: "Id",
                keyValue: new Guid("3f2504e0-4f89-41d3-9a0c-0305e82c3301"),
                column: "PosterUrl",
                value: "https://image.tmdb.org/t/p/w500/q6y0Go1tsGEsmtFryDOJo3dEmqu.jpg"
            );

            migrationBuilder.UpdateData(
                table: "movies",
                keyColumn: "Id",
                keyValue: new Guid("3f2504e0-4f89-41d3-9a0c-0305e82c3302"),
                column: "PosterUrl",
                value: "https://image.tmdb.org/t/p/w500/3bhkrj58Vtu7enYsRolD1fZdja1.jpg"
            );

            migrationBuilder.UpdateData(
                table: "movies",
                keyColumn: "Id",
                keyValue: new Guid("3f2504e0-4f89-41d3-9a0c-0305e82c3303"),
                column: "PosterUrl",
                value: "https://image.tmdb.org/t/p/w500/7IiTTgloJzvGI1TAYymCfbfl3vT.jpg"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "PosterUrl", table: "movies");
        }
    }
}
