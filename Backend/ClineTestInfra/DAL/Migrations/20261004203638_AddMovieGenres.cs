using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ClineTestInfra.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddMovieGenres : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder
                .CreateTable(
                    name: "genres",
                    columns: table => new
                    {
                        Name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false).Annotation("MySql:CharSet", "utf8mb4"),
                    },
                    constraints: table =>
                    {
                        table.PrimaryKey("PK_genres", x => x.Name);
                    }
                )
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder
                .CreateTable(
                    name: "movie_genres",
                    columns: table => new
                    {
                        MoviesId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                        GenreName = table.Column<string>(type: "varchar(100)", nullable: false).Annotation("MySql:CharSet", "utf8mb4"),
                    },
                    constraints: table =>
                    {
                        table.PrimaryKey("PK_movie_genres", x => new { x.MoviesId, x.GenreName });
                        table.ForeignKey(
                            name: "FK_movie_genres_genres",
                            column: x => x.GenreName,
                            principalTable: "genres",
                            principalColumn: "Name",
                            onDelete: ReferentialAction.Cascade
                        );
                        table.ForeignKey(
                            name: "FK_movie_genres_movies",
                            column: x => x.MoviesId,
                            principalTable: "movies",
                            principalColumn: "Id",
                            onDelete: ReferentialAction.Cascade
                        );
                    }
                )
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.InsertData(table: "genres", column: "Name", values: new object[] { "Crime", "Drama", "Thriller" });

            migrationBuilder.InsertData(
                table: "movie_genres",
                columns: new[] { "GenreName", "MoviesId" },
                values: new object[,]
                {
                    { "Crime", new Guid("3f2504e0-4f89-41d3-9a0c-0305e82c3301") },
                    { "Drama", new Guid("3f2504e0-4f89-41d3-9a0c-0305e82c3301") },
                    { "Crime", new Guid("3f2504e0-4f89-41d3-9a0c-0305e82c3302") },
                    { "Drama", new Guid("3f2504e0-4f89-41d3-9a0c-0305e82c3302") },
                    { "Drama", new Guid("3f2504e0-4f89-41d3-9a0c-0305e82c3303") },
                    { "Thriller", new Guid("3f2504e0-4f89-41d3-9a0c-0305e82c3303") },
                }
            );

            migrationBuilder.CreateIndex(name: "IX_movie_genres_GenreName", table: "movie_genres", column: "GenreName");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "movie_genres");

            migrationBuilder.DropTable(name: "genres");
        }
    }
}
