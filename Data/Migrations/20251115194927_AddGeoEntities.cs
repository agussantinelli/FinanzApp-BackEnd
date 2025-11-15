using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class AddGeoEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "FechaAlta",
                table: "Personas",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "SYSDATETIME()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaNac",
                table: "Personas",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "LocalidadId",
                table: "Personas",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PaisNacionalidadId",
                table: "Personas",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PaisResidenciaId",
                table: "Personas",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ProvinciaId",
                table: "Personas",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Paises",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    CodigoIso2 = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                    CodigoIso3 = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    EsArgentina = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Paises", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Provincias",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    PaisId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Provincias", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Provincias_Pais",
                        column: x => x.PaisId,
                        principalTable: "Paises",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Localidades",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    ProvinciaId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Localidades", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Localidades_Provincia",
                        column: x => x.ProvinciaId,
                        principalTable: "Provincias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Personas_LocalidadId",
                table: "Personas",
                column: "LocalidadId");

            migrationBuilder.CreateIndex(
                name: "IX_Personas_PaisNacionalidadId",
                table: "Personas",
                column: "PaisNacionalidadId");

            migrationBuilder.CreateIndex(
                name: "IX_Personas_PaisResidenciaId",
                table: "Personas",
                column: "PaisResidenciaId");

            migrationBuilder.CreateIndex(
                name: "IX_Personas_ProvinciaId",
                table: "Personas",
                column: "ProvinciaId");

            migrationBuilder.CreateIndex(
                name: "IX_Localidades_ProvinciaId",
                table: "Localidades",
                column: "ProvinciaId");

            migrationBuilder.CreateIndex(
                name: "UX_Paises_Iso2",
                table: "Paises",
                column: "CodigoIso2",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_Paises_Iso3",
                table: "Paises",
                column: "CodigoIso3",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Provincias_PaisId",
                table: "Provincias",
                column: "PaisId");

            migrationBuilder.AddForeignKey(
                name: "FK_Personas_Localidad",
                table: "Personas",
                column: "LocalidadId",
                principalTable: "Localidades",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Personas_PaisNacionalidad",
                table: "Personas",
                column: "PaisNacionalidadId",
                principalTable: "Paises",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Personas_PaisResidencia",
                table: "Personas",
                column: "PaisResidenciaId",
                principalTable: "Paises",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Personas_Provincia",
                table: "Personas",
                column: "ProvinciaId",
                principalTable: "Provincias",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Personas_Localidad",
                table: "Personas");

            migrationBuilder.DropForeignKey(
                name: "FK_Personas_PaisNacionalidad",
                table: "Personas");

            migrationBuilder.DropForeignKey(
                name: "FK_Personas_PaisResidencia",
                table: "Personas");

            migrationBuilder.DropForeignKey(
                name: "FK_Personas_Provincia",
                table: "Personas");

            migrationBuilder.DropTable(
                name: "Localidades");

            migrationBuilder.DropTable(
                name: "Provincias");

            migrationBuilder.DropTable(
                name: "Paises");

            migrationBuilder.DropIndex(
                name: "IX_Personas_LocalidadId",
                table: "Personas");

            migrationBuilder.DropIndex(
                name: "IX_Personas_PaisNacionalidadId",
                table: "Personas");

            migrationBuilder.DropIndex(
                name: "IX_Personas_PaisResidenciaId",
                table: "Personas");

            migrationBuilder.DropIndex(
                name: "IX_Personas_ProvinciaId",
                table: "Personas");

            migrationBuilder.DropColumn(
                name: "FechaNac",
                table: "Personas");

            migrationBuilder.DropColumn(
                name: "LocalidadId",
                table: "Personas");

            migrationBuilder.DropColumn(
                name: "PaisNacionalidadId",
                table: "Personas");

            migrationBuilder.DropColumn(
                name: "PaisResidenciaId",
                table: "Personas");

            migrationBuilder.DropColumn(
                name: "ProvinciaId",
                table: "Personas");

            migrationBuilder.AlterColumn<DateTime>(
                name: "FechaAlta",
                table: "Personas",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "SYSDATETIME()");
        }
    }
}
