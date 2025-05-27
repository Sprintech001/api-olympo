using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace olympo_webapi.Migrations.Connection
{
    /// <inheritdoc />
    public partial class Initial1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: 5);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Exercises",
                columns: new[] { "Id", "Description", "ImagePath", "Name", "VideoPath" },
                values: new object[,]
                {
                    { 1, "Descrição do exercício", "images/exe2.png", "Agachamento Terra", "videos/execucao.mp4" },
                    { 2, "Descrição do exercício", "images/exe.png", "Rosca Concentrada", "videos/execucao.mp4" },
                    { 3, "Descrição do exercício", "images/exe3.png", "Supino Reto", "videos/execucao.mp4" },
                    { 4, "Descrição do exercício", "images/exe4.png", "Puxada Aberta", "videos/execucao.mp4" },
                    { 5, "Descrição do exercício", "images/exe5.png", "Levantamento Terra", "videos/execucao.mp4" }
                });
        }
    }
}
