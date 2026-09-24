using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ClineTestInfra.DAL.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "movies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Title = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Year = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Director = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Overview = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OriginalLanguage = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_movies", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.InsertData(
                table: "movies",
                columns: new[] { "Id", "Description", "Director", "OriginalLanguage", "Overview", "Title", "Year" },
                values: new object[,]
                {
                    { new Guid("3f2504e0-4f89-41d3-9a0c-0305e82c3301"), "Two imprisoned men bond over a number of years.", "Frank Darabont", "English", "A banker convicted of uxoricide forms a friendship over a quarter century with a hardened convict.", "The Shawshank Redemption", 1994 },
                    { new Guid("3f2504e0-4f89-41d3-9a0c-0305e82c3302"), "The aging patriarch of an organized crime dynasty transfers control to his reluctant son.", "Francis Ford Coppola", "English", "The story of the Corleone family under patriarch Vito Corleone.", "The Godfather", 1972 },
                    { new Guid("3f2504e0-4f89-41d3-9a0c-0305e82c3303"), "Greed and class discrimination threaten the newly formed symbiotic relationship between two families.", "Bong Joon-ho", "Korean", "A poor family schemes to become employed by a wealthy family by infiltrating their household.", "Parasite", 2019 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "movies");
        }
    }
}
