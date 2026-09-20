using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SeedTopics : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var at = new DateTimeOffset(2026, 1, 12, 12, 0, 0, TimeSpan.Zero);
            migrationBuilder.InsertData(
                table: "Topics",
                columns: new[] { "Id", "Name", "Description", "OrderNumber", "IsActive", "CreatedAt", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-111111111111"), "Fundamentos de programación", "Entrada, salida, condicionales, ciclos y funciones.", 1, true, at, at },
                    { new Guid("22222222-2222-2222-2222-222222222222"), "Estructuras de datos básicas", "Arreglos, listas, pilas, colas y diccionarios.", 2, true, at, at },
                    { new Guid("33333333-3333-3333-3333-333333333333"), "Técnicas algorítmicas iniciales", "Búsqueda, ordenamiento y dos punteros.", 3, true, at, at },
                    { new Guid("44444444-4444-4444-4444-444444444444"), "Recursión y backtracking", "Recursión, casos base y exploración con retroceso.", 4, true, at, at },
                    { new Guid("55555555-5555-5555-5555-555555555555"), "Optimización y complejidad, Big O", "Análisis de complejidad y optimización.", 5, true, at, at },
                    { new Guid("66666666-6666-6666-6666-666666666666"), "Preparación para competencias y entrevistas técnicas", "Estrategia de concurso y problemas de entrevista.", 6, true, at, at }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(table: "Topics", keyColumn: "Id", keyValues: new object[]
            {
                new Guid("11111111-1111-1111-1111-111111111111"),
                new Guid("22222222-2222-2222-2222-222222222222"),
                new Guid("33333333-3333-3333-3333-333333333333"),
                new Guid("44444444-4444-4444-4444-444444444444"),
                new Guid("55555555-5555-5555-5555-555555555555"),
                new Guid("66666666-6666-6666-6666-666666666666")
            });
        }
    }
}
